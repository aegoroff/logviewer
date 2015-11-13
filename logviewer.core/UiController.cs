// Created by: egr
// Created at: 07.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using logviewer.core.Properties;
using logviewer.engine;
using Ninject;
using LogLevel = logviewer.engine.LogLevel;


namespace logviewer.core
{
    public sealed class UiController : BaseGuiController, IDisposable
    {
        private readonly ISettingsProvider settings;

        #region Constants and Fields

        private readonly Dictionary<LogLevel, ulong> byLevel = new Dictionary<LogLevel, ulong>();
        private readonly IDictionary<string, Encoding> filesEncodingCache = new ConcurrentDictionary<string, Encoding>();

        private readonly IDictionary<Task, string> runningTasks = new ConcurrentDictionary<Task, string>();

        private CancellationTokenSource cancellation = new CancellationTokenSource();
        private readonly LogCharsetDetector charsetDetector = new LogCharsetDetector();

        private string currentPath;

        private GrokMatcher matcher;
        private GrokMatcher filter;
        private LogStore store;
        private long totalMessages;
        private readonly IViewModel viewModel;

        private readonly ProducerConsumerQueue queue =
            new ProducerConsumerQueue(Math.Max(2, Environment.ProcessorCount / 2));

        private readonly Stopwatch probeWatch = new Stopwatch();
        private readonly Stopwatch totalReadTimeWatch = new Stopwatch();
        private readonly TimeSpan filterUpdateDelay = TimeSpan.FromMilliseconds(200);
        private readonly RegexOptions options;
        private readonly IKernel kernel;
        private readonly TaskScheduler uiSyncContext;

        #endregion

        #region Constructors and Destructors

        public UiController(IViewModel viewModel, RegexOptions options = RegexOptions.ExplicitCapture)
        {
            this.settings = viewModel.SettingsProvider;
            this.viewModel = viewModel;
            this.prevInput = DateTime.Now;
            this.options = options;
            this.kernel = new StandardKernel(new CoreModule());
            viewModel.PropertyChanged += this.ViewModelOnPropertyChanged;
            this.uiSyncContext = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case nameof(this.viewModel.From):
                case nameof(this.viewModel.To):
                case nameof(this.viewModel.MinLevel):
                case nameof(this.viewModel.MaxLevel):
                case nameof(this.viewModel.MessageFilter):
                case nameof(this.viewModel.SortingOrder):
                case nameof(this.viewModel.UseRegularExpressions):
                    this.StartReadingLogOnTextFilterChange();
                    break;
            }
        }

        private void SetCurrentParsingTemplate()
        {
            var template = this.settings.ReadParsingTemplate();
            this.CreateMessageHead(template.StartMessage, template.Compiled);
            this.CreateMessageFilter(template.Filter);
        }

        private void CreateMessageHead(string startMessagePattern, bool compiled)
        {
            this.matcher = new GrokMatcher(startMessagePattern, compiled ? this.options | RegexOptions.Compiled : this.options);
        }
        
        private void CreateMessageFilter(string messageFilter)
        {
            this.filter = string.IsNullOrWhiteSpace(messageFilter) ? null : new GrokMatcher(messageFilter);
        }

        #endregion

        #region Public Methods and Operators

        private bool NotCancelled => !this.cancellation.IsCancellationRequested;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void CancelPreviousTask()
        {
            if (!this.cancellation.IsCancellationRequested)
            {
                this.viewModel.LogProgressText = Resources.CancelPrevious;
                SafeRunner.Run(this.cancellation.Cancel);
            }
            this.queue.CleanupPendingTasks();
            this.WaitRunningTasks();
            SafeRunner.Run(this.cancellation.Dispose);
            this.DisposeRunningTasks();
            this.runningTasks.Clear();
        }

        private void DisposeRunningTasks()
        {
            foreach (
                var task in
                    this.runningTasks.Keys.Where(
                        task =>
                            task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Faulted ||
                            task.Status == TaskStatus.Canceled))
            {
                SafeRunner.Run(task.Dispose);
            }
        }

        private void WaitRunningTasks()
        {
            foreach (
                var task in
                    this.runningTasks.Keys.Where(
                        task =>
                            task.Status == TaskStatus.Running || task.Status == TaskStatus.WaitingForChildrenToComplete ||
                            task.Status == TaskStatus.WaitingForActivation))
            {
                SafeRunner.Run(task.Wait);
            }
        }

        private DateTime prevInput;

        public bool PendingStart { get; private set; }

        public void StartReadingLogOnTextFilterChange()
        {
            if (!IsValidFilter(this.viewModel.MessageFilter, this.viewModel.UseRegularExpressions))
            {
                return;
            }

            this.prevInput = DateTime.Now;

            if (this.PendingStart || !this.viewModel.UiControlsEnabled)
            {
                return;
            }
            Task.Factory.StartNew(delegate
            {
                this.PendingStart = true;
                try
                {
                    SpinWait.SpinUntil(() =>
                    {
                        var diff = DateTime.Now - this.prevInput;
                        return diff > this.filterUpdateDelay;
                    });
                    this.UpdateRecentFilters(this.viewModel.MessageFilter);
                    this.StartLogReadingTask();
                }
                finally
                {
                    this.PendingStart = false;
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static bool IsValidFilter(string filter, bool regexp)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }
            if (!regexp)
            {
                return true;
            }
            try
            {
                var r = new Regex(filter, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                return r.GetHashCode() > 0;
            }
            catch (Exception e)
            {
                Log.Instance.Info(e.Message, e);
                return false;
            }
        }

        private void StartLogReadingTask()
        {
            this.cancellation = new CancellationTokenSource();
            var task = Task.Factory.StartNew(this.DoLogReadingTask, this.cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            Action<Task> onSuccess = obj => this.OnComplete(task, () => {});
            Action<Task> onFailure = delegate
            {
                this.OnComplete(task, () => this.viewModel.UiControlsEnabled = true);
            };

            task.ContinueWith(onSuccess, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, this.uiSyncContext);
            task.ContinueWith(onSuccess, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, this.uiSyncContext);
            task.ContinueWith(onFailure, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, this.uiSyncContext);

            this.runningTasks.Add(task, this.viewModel.LogPath);
        }

        private void OnComplete(Task task, Action action)
        {
            try
            {
                action();
            }
            finally
            {
                if (this.runningTasks.ContainsKey(task))
                {
                    this.runningTasks.Remove(task);
                    task.Dispose();
                }
            }
        }

        public void UpdateLog(string path)
        {
            if (string.IsNullOrWhiteSpace(path) ||
                !path.Equals(this.currentPath, StringComparison.CurrentCultureIgnoreCase) || !this.settings.AutoRefreshOnFileChange)
            {
                return;
            }
            Action action = delegate
            {
                this.WaitRunningTasks();
                var f = new FileInfo(path);
                if (f.Length < this.logSize)
                {
                    this.currentPath = string.Empty;
                    this.logSize = 0;
                    this.store.Do(logStore =>
                    {
                        logStore.Dispose();
                        this.store = null;
                    });
                }
                if (f.Length == this.logSize)
                {
                    return;
                }
                this.StartLogReadingTask();
            };
            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void UpdateRecentFilters(string value = null)
        {
            this.settings.UseRecentFiltersStore(delegate(RecentItemsStore itemsStore)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    itemsStore.Add(value);
                }
            });
        }

        /// <summary>
        ///     Reads log from file
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect")]
        private void DoLogReadingTask()
        {
            this.viewModel.UiControlsEnabled = false;
            this.totalReadTimeWatch.Restart();
            if (this.viewModel.MinLevel > this.viewModel.MaxLevel && (LogLevel)this.viewModel.MaxLevel >= LogLevel.Trace)
            {
                throw new ArgumentException(Resources.MinLevelGreaterThenMax);
            }
            var reader = new LogReader(this.charsetDetector, this.matcher, this.filter);
            var logPath = this.viewModel.LogPath;
            
            var length = new FileInfo(logPath).Length;

            var append = length > this.logSize && this.CurrentPathCached;

            var offset = append ? this.logSize : 0L;

            this.logSize = length;

            if (this.logSize == 0)
            {
                throw new ArgumentException(Resources.ZeroFileDetected);
            }

            if (this.CurrentPathCached && !append)
            {
                this.AfterDatabaseCreation(true);
                return;
            }

            this.currentPath = logPath;
            if (this.currentPath == null)
            {
                return;
            }

            this.viewModel.LogSize = new FileSize(this.logSize, true).Format();
            this.ResetLogStatistic();

            var dbSize = this.logSize + (this.logSize / 10) * 4; // +40% to log file
            if (this.store != null && !append)
            {
                this.store.Dispose();
            }
            if (!append || this.store == null)
            {
                this.store = new LogStore(dbSize, null, this.matcher.MessageSchema);
            }
            GC.Collect();
            this.store.StartAddMessages();
            if (!append)
            {
                this.totalMessages = 0;
            }
            reader.ProgressChanged += this.OnReadLogProgressChanged;
            reader.CompilationStarted += this.OnCompilationStarted;
            reader.CompilationFinished += this.OnCompilationFinished;
            reader.EncodingDetectionStarted += this.OnEncodingDetectionStarted;
            reader.EncodingDetectionFinished += this.OnEncodingDetectionFinished;
            Encoding inputEncoding;
            this.filesEncodingCache.TryGetValue(this.currentPath, out inputEncoding);
            try
            {
                this.queuedMessages = 0;
                reader.Read(logPath, this.AddMessageToCache, () => this.NotCancelled, ref inputEncoding, offset);
                this.probeWatch.Stop();
                var elapsed = this.probeWatch.Elapsed;
                var pending = Interlocked.Read(ref this.queuedMessages);
                var inserted = this.totalMessages - pending;
                var insertRatio = inserted / elapsed.TotalSeconds;
                var remain = Math.Abs(insertRatio) < 0.00001
                    ? TimeSpan.FromSeconds(0)
                    : TimeSpan.FromSeconds(pending / insertRatio);

                if (this.currentPath != null && !this.filesEncodingCache.ContainsKey(this.currentPath) &&
                    inputEncoding != null)
                {
                    this.filesEncodingCache.Add(this.currentPath, inputEncoding);
                }
                var remainSeconds = remain.Seconds;
                if (remainSeconds > 0)
                {
                    this.viewModel.LogProgressText = string.Format(Resources.FinishLoading, remainSeconds);
                }
                // Interlocked is a must because other threads can change this
                SpinWait.SpinUntil(
                    () => Interlocked.Read(ref this.queuedMessages) == 0 || this.cancellation.IsCancellationRequested);
            }
            finally
            {
                this.viewModel.LogProgressText = Resources.LogIndexing;
                this.store.FinishAddMessages();
                reader.ProgressChanged -= this.OnReadLogProgressChanged;
                reader.CompilationStarted -= this.OnCompilationStarted;
                reader.CompilationFinished -= this.OnCompilationFinished;
                reader.EncodingDetectionStarted -= this.OnEncodingDetectionStarted;
                reader.EncodingDetectionFinished -= this.OnEncodingDetectionFinished;
            }
            for (var i = 0; i < (int)LogLevel.Fatal + 1; i++)
            {
                var level = (LogLevel)i;
                this.byLevel[level] = (ulong)this.store.CountMessages(level, level, this.viewModel.MessageFilter, this.viewModel.UseRegularExpressions, true);
            }
            this.AfterDatabaseCreation(false);
        }

        private void OnCompilationFinished(object sender, EventArgs eventArgs)
        {
            this.viewModel.LogProgressText = Resources.PatternCompilationFinished;
        }

        private void OnCompilationStarted(object sender, EventArgs eventArgs)
        {
            this.viewModel.LogProgressText = Resources.PatternCompilation;
        }

        /// <remarks>
        /// this method MUST be called only from one thread.
        /// </remarks>
        private void AddMessageToCache(LogMessage message)
        {
            if (message.IsEmpty || this.cancellation.IsCancellationRequested)
            {
                return;
            }
            // Interlocked is a must because other threads can change this
            Interlocked.Increment(ref this.queuedMessages);
            message.Ix = Interlocked.Increment(ref this.totalMessages);

            this.queue.EnqueueItem(delegate
            {
                try
                {
                    this.store.AddMessage(message);
                }
                finally
                {
                    // Interlocked is a must because other threads can change this
                    Interlocked.Decrement(ref this.queuedMessages);
                }
            });
        }

        private void OnEncodingDetectionFinished(object sender, EncodingDetectedEventArgs e)
        {
            this.probeWatch.Restart();
            this.viewModel.LogProgressText = string.Empty;
            this.viewModel.LogEncoding = e.ToString();
        }

        private void OnEncodingDetectionStarted(object sender, EventArgs e)
        {
            this.viewModel.LogEncoding = string.Empty;
            this.viewModel.LogProgressText = Resources.EncodingDetectionInProgress;
        }

        private void AfterDatabaseCreation(bool cached)
        {
            if (!cached)
            {
                this.viewModel.From = this.SelectDateUsingFunc("min");
                this.viewModel.To = this.SelectDateUsingFunc("max");
            }

            this.viewModel.Provider.Filter = new MessageFilter
            {
                Filter = this.viewModel.MessageFilter,
                Finish = this.viewModel.To,
                Start = this.viewModel.From,
                Min = (LogLevel) this.viewModel.MinLevel,
                Max = (LogLevel) this.viewModel.MaxLevel,
                UseRegexp = this.viewModel.UseRegularExpressions,
                Reverse = this.viewModel.SortingOrder == 0
            };
            this.viewModel.Provider.Store = this.store;
            this.viewModel.Datasource.Clear();

            this.viewModel.UiControlsEnabled = true;

            this.ShowLogPageStatistic();
            this.ShowElapsedTime();
        }

        private DateTime SelectDateUsingFunc(string func)
        {
            return this.store.SelectDateUsingFunc(func, LogLevel.Trace, LogLevel.Fatal, this.viewModel.MessageFilter, this.viewModel.UseRegularExpressions);
        }

        private void ShowElapsedTime()
        {
            this.totalReadTimeWatch.Stop();
            var text = string.Format(Resources.ReadCompletedTemplate, this.totalReadTimeWatch.Elapsed.TimespanToHumanString());
            this.viewModel.LogProgressText = text;
        }

        private void ShowLogPageStatistic()
        {
            this.viewModel.TotalMessages = ToHumanReadableString((ulong) this.TotalMessages);
            this.viewModel.ToDisplayMessages = ToHumanReadableString((ulong)this.viewModel.Provider.FetchCount());
            this.viewModel.LogStatistic = string.Format(Resources.LoStatisticFormatString,
                this.CountMessages(LogLevel.Trace),
                this.CountMessages(LogLevel.Debug),
                this.CountMessages(LogLevel.Info),
                this.CountMessages(LogLevel.Warn),
                this.CountMessages(LogLevel.Error),
                this.CountMessages(LogLevel.Fatal)
                );
        }

        private static string ToHumanReadableString(ulong value)
        {
            var formatTotal = value.FormatString();
            return value.ToString(formatTotal, CultureInfo.CurrentCulture);
        }

        private void OnReadLogProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.OnLogReadProgress(e.UserState);
        }

        private void OnLogReadProgress(object progress)
        {
            var formatTotal = ((ulong) this.totalMessages).FormatString();
            var total = this.totalMessages.ToString(formatTotal, CultureInfo.CurrentCulture);

            var logProgress = (LoadProgress) progress;
            this.viewModel.LogProgress = logProgress.Percent;
            this.viewModel.LogProgressText = logProgress.Format();
            this.ChangeTotalOnUi(total);
        }

        private void ChangeTotalOnUi(string total)
        {
            this.viewModel.TotalMessages = total;
            this.viewModel.ToDisplayMessages = "0";
            this.viewModel.LogStatistic = string.Format(Resources.LoStatisticFormatString, 0, 0, 0, 0, 0, 0);
        }

        private void ResetLogStatistic()
        {
            this.ChangeTotalOnUi("0");
        }

        public void CancelReading()
        {
            if (this.cancellation.IsCancellationRequested)
            {
                return;
            }
            this.cancellation.Cancel();
        }

        public string GetLogSize(bool showBytes)
        {
            return new FileSize(this.logSize, !showBytes).Format();
        }

        public string CurrentEncoding => this.filesEncodingCache.ContainsKey(this.currentPath)
            ? this.filesEncodingCache[this.currentPath].EncodingName
            : string.Empty;

        public void ClearCache()
        {
            this.currentPath = null;
            this.SetCurrentParsingTemplate();
        }

        public void LoadLastOpenedFile()
        {
            if (!this.settings.OpenLastFile)
            {
                return;
            }

            var lastOpenedFile = string.Empty;

            this.settings.UseRecentFilesStore(filesStore => lastOpenedFile = filesStore.ReadLastUsedItem());

            if (string.IsNullOrWhiteSpace(lastOpenedFile))
            {
                return;
            }
            this.ReadNewLog();
        }

        public void ReadNewLog()
        {
            this.CancelPreviousTask();
            this.ClearCache();
            this.StartLogReadingTask();
        }

        public void UpdateSettings(bool refresh)
        {
            var template = this.settings.ReadParsingTemplate();
            if (!template.IsEmpty)
            {
                this.CreateMessageHead(template.StartMessage, template.Compiled);
                this.CreateMessageFilter(template.Filter);
            }
            if (refresh)
            {
                this.StartLogReadingTask();
            }
        }

        public void AddCurrentFileToRecentFilesList()
        {
            this.settings.UseRecentFilesStore(s => s.Add(this.viewModel.LogPath));
        }

        public string CountMessages(LogLevel level)
        {
            return ToHumanReadableString(this.byLevel.ContainsKey(level) ? this.byLevel[level] : 0);
        }

        #endregion

        #region Methods

        private long logSize;

        private bool CurrentPathCached => !string.IsNullOrWhiteSpace(this.currentPath) &&
                                          this.currentPath.Equals(this.viewModel.LogPath, StringComparison.CurrentCultureIgnoreCase);

        private long TotalMessages => this.store?.CountMessages() ?? 0;

        public LogStore Store => this.store;

        private long queuedMessages;

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed",
            MessageId = "cancellation"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed",
             MessageId = "store")]
        public void Dispose()
        {
            this.queue.Shutdown(true);
            try
            {
                SafeRunner.Run(this.CancelPreviousTask);
            }
            finally
            {
                this.store.Do(logStore => SafeRunner.Run(logStore.Dispose));
                this.kernel.Dispose();
            }
        }
    }
}