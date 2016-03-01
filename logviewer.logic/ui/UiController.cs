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
using Humanizer;
using logviewer.engine;
using logviewer.logic.models;
using logviewer.logic.Properties;
using logviewer.logic.storage;
using logviewer.logic.support;
using LogLevel = logviewer.engine.LogLevel;


namespace logviewer.logic.ui
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
        private LogReader reader;

        private string currentPath;

        private MessageMatcher matcher;
        private LogStore store;
        private long totalMessages;
        private readonly IViewModel viewModel;

        private readonly ProducerConsumerQueue queue =
            new ProducerConsumerQueue(Math.Max(2, Environment.ProcessorCount / 2));

        private readonly Stopwatch probeWatch = new Stopwatch();
        private readonly Stopwatch totalReadTimeWatch = new Stopwatch();
        private readonly TimeSpan filterUpdateDelay = TimeSpan.FromMilliseconds(200);
        public event EventHandler<EventArgs> ReadCompleted;
        private const int CheckUpdatesEveryDays = 7;

        #endregion

        #region Constructors and Destructors

        public UiController(IViewModel viewModel)
        {
            this.settings = viewModel.SettingsProvider;
            this.viewModel = viewModel;
            this.VersionsReader = new VersionsReader(this.viewModel.GithubAccount, this.viewModel.GithubProject);
            this.prevInput = DateTime.Now;
            viewModel.PropertyChanged += this.ViewModelOnPropertyChanged;
        }

        private VersionsReader VersionsReader { get; }

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
                case nameof(this.viewModel.Visible):
                    this.UpdateCount();
                    break;
            }
        }

        private void UpdateCount()
        {
            this.viewModel.Datasource.ChangeVisible(this.viewModel.Visible);
        }

        public void UpdateSettings(bool refresh)
        {
            this.matcher = new MessageMatcher(this.settings.ReadParsingTemplate(), RegexOptions.ExplicitCapture);
            if (refresh)
            {
                this.StartLogReadingTask();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Check updates available
        /// </summary>
        /// <param name="manualInvoke">Whether to show no update available in GUI. False by default</param>
        public void CheckUpdates(bool manualInvoke = false)
        {
            if (!manualInvoke)
            {
                if (DateTime.UtcNow < this.settings.LastUpdateCheckTime.AddDays(CheckUpdatesEveryDays))
                {
                    return;
                }
            }
            var checker = new UpdatesChecker(this.VersionsReader);
            this.settings.LastUpdateCheckTime = DateTime.UtcNow;
            Task.Factory.StartNew(delegate
            {
                if (!checker.IsUpdatesAvaliable())
                {
                    if (manualInvoke)
                    {
                        this.RunOnGuiThread(() => this.viewModel.ShowNoUpdateAvaliable());
                    }
                    return;
                }

                this.RunOnGuiThread(
                    () =>
                        this.viewModel.ShowDialogAboutNewVersionAvaliable(checker.CurrentVersion, checker.LatestVersion,
                            checker.LatestVersionUrl));
            });
        }

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
            if (!this.viewModel.MessageFilter.IsValid(this.viewModel.UseRegularExpressions))
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

        

        private void StartLogReadingTask()
        {
            this.cancellation = new CancellationTokenSource();
            var task = Task.Factory.StartNew(this.DoLogReadingTask, this.cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            Action<Task> onSuccess = obj => this.OnComplete(task);
            Action<Task> onFailure = delegate
            {
                this.OnComplete(task);
            };

            task.ContinueWith(onSuccess, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, this.UiSyncContext);
            task.ContinueWith(onSuccess, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, this.UiSyncContext);
            task.ContinueWith(onFailure, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, this.UiSyncContext);

            this.runningTasks.Add(task, this.viewModel.LogPath);
        }

        private void OnComplete(Task task)
        {
            try
            {
                if (!this.cancellation.IsCancellationRequested)
                {
                    this.viewModel.Datasource.LoadCount((int) this.viewModel.Provider.FetchCount());
                }
                this.viewModel.UiControlsEnabled = true;
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
            this.reader = new LogReader(this.charsetDetector, this.matcher);
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
            this.ChangeTotalOnUi("0"); // Not L10N

            var dbSize = this.logSize + (this.logSize / 10) * 4; // +40% to log file
            if (this.store != null && !append)
            {
                this.store.Dispose();
            }
            if (!append || this.store == null)
            {
                this.store = new LogStore(dbSize, null, this.matcher.IncludeMatcher.MessageSchema);
            }
            GC.Collect();
            this.store.StartAddMessages();
            if (!append)
            {
                this.totalMessages = 0;
            }
            this.reader.ProgressChanged += this.OnReadLogProgressChanged;
            this.reader.CompilationStarted += this.OnCompilationStarted;
            this.reader.CompilationFinished += this.OnCompilationFinished;
            this.reader.EncodingDetectionStarted += this.OnEncodingDetectionStarted;
            this.reader.EncodingDetectionFinished += this.OnEncodingDetectionFinished;
            Encoding inputEncoding;
            this.filesEncodingCache.TryGetValue(this.currentPath, out inputEncoding);
            try
            {
                this.queuedMessages = 0;

                using (var enumerator = this.reader.Read(logPath, inputEncoding, offset).GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        this.AddMessageToCache(enumerator.Current);
                    }
                }

                this.probeWatch.Stop();
                if (!this.NotCancelled)
                {
                    return;
                }
                var elapsed = this.probeWatch.Elapsed;
                var pending = Interlocked.Read(ref this.queuedMessages);
                var inserted = this.totalMessages - pending;
                var insertRatio = inserted / elapsed.TotalSeconds;
                var remain = Math.Abs(insertRatio) < 0.00001
                    ? TimeSpan.FromSeconds(0)
                    : TimeSpan.FromSeconds(pending / insertRatio);

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
                if (this.NotCancelled)
                {
                    this.store.FinishAddMessages();
                }
                this.reader.ProgressChanged -= this.OnReadLogProgressChanged;
                this.reader.CompilationStarted -= this.OnCompilationStarted;
                this.reader.CompilationFinished -= this.OnCompilationFinished;
                this.reader.EncodingDetectionStarted -= this.OnEncodingDetectionStarted;
                this.reader.EncodingDetectionFinished -= this.OnEncodingDetectionFinished;
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

            if (this.currentPath != null && !this.filesEncodingCache.ContainsKey(this.currentPath) && e.Encoding != null)
            {
                this.filesEncodingCache.Add(this.currentPath, e.Encoding);
            }

        }

        private void OnEncodingDetectionStarted(object sender, EventArgs e)
        {
            this.viewModel.LogEncoding = string.Empty;
            this.viewModel.LogProgressText = Resources.EncodingDetectionInProgress;
        }

        private void AfterDatabaseCreation(bool cached)
        {
            if (this.cancellation.IsCancellationRequested)
            {
                return;
            }
            if (!cached)
            {
                this.viewModel.From = this.SelectDateUsingFunc("min"); // Not L10N
                this.viewModel.To = this.SelectDateUsingFunc("max"); // Not L10N
            }

            this.viewModel.Provider.FilterModel = new MessageFilterModel
            {
                Filter = this.viewModel.MessageFilter,
                Finish = this.viewModel.To,
                Start = this.viewModel.From,
                Min = (LogLevel) this.viewModel.MinLevel,
                Max = (LogLevel) this.viewModel.MaxLevel,
                UseRegexp = this.viewModel.UseRegularExpressions,
                Reverse = this.viewModel.SortingOrder == 0
            };

            this.viewModel.TotalMessages = ToHumanReadableString((ulong) this.TotalMessages);
            
            this.totalReadTimeWatch.Stop();
            var text = string.Format(Resources.ReadCompletedTemplate, this.totalReadTimeWatch.Elapsed.Humanize());
            this.viewModel.LogProgressText = text;

            this.viewModel.Provider.Store = this.store;
            this.viewModel.Datasource.Clear();

            this.viewModel.ToDisplayMessages = this.viewModel.Provider.FetchCount().ToString("N0", CultureInfo.CurrentCulture); // Not L10N
            this.viewModel.LogStatistic = string.Format(Resources.LoStatisticFormatString,
                this.CountMessages(LogLevel.Trace),
                this.CountMessages(LogLevel.Debug),
                this.CountMessages(LogLevel.Info),
                this.CountMessages(LogLevel.Warn),
                this.CountMessages(LogLevel.Error),
                this.CountMessages(LogLevel.Fatal)
                );

            this.ReadCompleted.Do(handler => handler(this, new EventArgs()));
        }

        private DateTime SelectDateUsingFunc(string func)
        {
            return this.store.SelectDateUsingFunc(func, LogLevel.Trace, LogLevel.Fatal, this.viewModel.MessageFilter, this.viewModel.UseRegularExpressions);
        }

        private static string ToHumanReadableString(ulong value)
        {
            return value.ToString("N0", CultureInfo.CurrentCulture); // Not L10N
        }

        private void OnReadLogProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.OnLogReadProgress(e.UserState);
        }

        private void OnLogReadProgress(object progress)
        {
            var total = this.totalMessages.ToString("N0", CultureInfo.CurrentCulture); // Not L10N

            var logProgress = (LoadProgress) progress;
            this.viewModel.LogProgress = logProgress.Percent;
            this.viewModel.LogProgressText = logProgress.Format();
            this.ChangeTotalOnUi(total);
        }

        private void ChangeTotalOnUi(string total)
        {
            this.viewModel.TotalMessages = total;
            this.viewModel.ToDisplayMessages = "0"; // Not L10N
            this.viewModel.LogStatistic = string.Format(Resources.LoStatisticFormatString, 0, 0, 0, 0, 0, 0);
        }

        public void CancelReading()
        {
            if (this.cancellation.IsCancellationRequested)
            {
                return;
            }
            this.cancellation.Cancel();
            this.reader?.Cancel();
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
            this.CancelReading();
            this.CancelPreviousTask();
            this.ClearCache();
            this.UpdateSettings(true);
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
            }
        }
    }
}