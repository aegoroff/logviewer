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
using Net.Sgoliver.NRtfTree.Util;
using Ninject;
using LogLevel = logviewer.engine.LogLevel;


namespace logviewer.core
{
    public sealed class UiController : BaseGuiController, IDisposable
    {
        private readonly ISettingsProvider settings;

        #region Constants and Fields

        private readonly Dictionary<LogLevel, int> byLevel = new Dictionary<LogLevel, int>();
        private readonly IDictionary<string, Encoding> filesEncodingCache = new ConcurrentDictionary<string, Encoding>();

        private readonly IDictionary<Task, string> runningTasks = new ConcurrentDictionary<Task, string>();

        private CancellationTokenSource cancellation = new CancellationTokenSource();
        private readonly LogCharsetDetector charsetDetector = new LogCharsetDetector();

        private string currentPath;

        private GrokMatcher matcher;
        private GrokMatcher filter;
        private int pageSize;
        private LogStore store;
        private long totalMessages;
        private readonly IViewModel viewModel;
        public event EventHandler<LogReadCompletedEventArgs> ReadCompleted;

        private readonly ProducerConsumerQueue queue =
            new ProducerConsumerQueue(Math.Max(2, Environment.ProcessorCount / 2));

        private readonly Stopwatch probeWatch = new Stopwatch();
        private readonly Stopwatch totalReadTimeWatch = new Stopwatch();
        private readonly TimeSpan filterUpdateDelay = TimeSpan.FromMilliseconds(200);
        private readonly RegexOptions options;
        private readonly IKernel kernel;

        #endregion

        #region Constructors and Destructors

        public UiController(IViewModel viewModel, RegexOptions options = RegexOptions.ExplicitCapture)
        {
            this.CurrentPage = 1;
            this.settings = viewModel.SettingsProvider;
            this.viewModel = viewModel;
            this.pageSize = this.settings.PageSize;
            this.prevInput = DateTime.Now;
            this.options = options;
            this.kernel = new StandardKernel(new CoreModule());
            viewModel.PropertyChanged += this.ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            switch (propertyChangedEventArgs.PropertyName)
            {
                case nameof(this.viewModel.From):
                case nameof(this.viewModel.To):
                case nameof(this.viewModel.MinLevel):
                case nameof(this.viewModel.MaxLevel):
                    this.StartReading();
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

        #region Public Properties

        public long MessagesCount
        {
            get
            {
                var start = (this.CurrentPage - 1) * this.pageSize;
                return this.totalFiltered - start;
            }
        }

        public int CurrentPage { get; set; }

        public int TotalPages
        {
            get
            {
                if (this.totalFiltered == 0)
                {
                    return 1;
                }
                return (int)Math.Ceiling(this.totalFiltered / (float)this.pageSize);
            }
        }

        public long DisplayedMessages
        {
            get
            {
                var finish = Math.Min(this.MessagesCount, this.pageSize);
                return Math.Min(finish, this.totalFiltered);
            }
        }

        #endregion

        #region Public Methods and Operators

        private bool NotCancelled => !this.cancellation.IsCancellationRequested;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void CancelPreviousTask()
        {
            if (!this.cancellation.IsCancellationRequested)
            {
                //this.view.SetLogProgressCustomText(Resources.CancelPrevious);
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

        public void StartReading()
        {
            if (!IsValidFilter(this.viewModel.MessageFilter, this.viewModel.UseRegularExpressions))
            {
                return;
            }

            this.UpdateMessageFilter(this.viewModel.MessageFilter);

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
                    this.BeginLogReading();
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

        public string CurrentEncoding => this.filesEncodingCache.ContainsKey(this.currentPath)
            ? this.filesEncodingCache[this.currentPath].EncodingName
            : string.Empty;

        private void BeginLogReading()
        {
            var errorMessage = string.Empty;
            Action action = delegate
            {
                try
                {
                    this.StartReadLog();
                }
                catch (Exception e)
                {
                    errorMessage = e.ToString();
                    throw;
                }
            };
            this.cancellation = new CancellationTokenSource();
            var task = Task.Factory.StartNew(action, this.cancellation.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            Action<Task> onSuccess = obj => this.OnComplete(task, () => {});
            Action<Task> onFailure = delegate
            {
                this.OnComplete(task, () => this.viewModel.UiControlsEnabled = true);
            };

            task.ContinueWith(onSuccess, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default);
            task.ContinueWith(onSuccess, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
            task.ContinueWith(onFailure, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
            
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
                this.BeginLogReading();
            };
            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void UpdateMessageFilter(string value)
        {
            this.settings.MessageFilter = value;
        }

        public void UpdateRecentFilters(string value = null)
        {
            string[] items = null;

            this.settings.UseRecentFiltersStore(delegate(RecentItemsStore itemsStore)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    itemsStore.Add(value);
                }

                items = itemsStore.ReadItems().ToArray();
            });

            //this.view.AddFilterItems(items);
        }

        /// <summary>
        ///     Reads log from file
        /// </summary>
        /// <returns>Path to RTF document to be loaded into control</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect")]
        public void StartReadLog()
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
                this.ReadLogFromInternalStore(true);
                return;
            }

            this.currentPath = logPath;
            if (this.currentPath == null)
            {
                return;
            }

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
                //var elapsed = this.probeWatch.Elapsed;
                //var pending = Interlocked.Read(ref this.queuedMessages);
                //var inserted = this.totalMessages - pending;
                //var insertRatio = inserted / elapsed.TotalSeconds;
                //var remain = Math.Abs(insertRatio) < 0.00001
                //    ? TimeSpan.FromSeconds(0)
                //    : TimeSpan.FromSeconds(pending / insertRatio);

                if (this.currentPath != null && !this.filesEncodingCache.ContainsKey(this.currentPath) &&
                    inputEncoding != null)
                {
                    this.filesEncodingCache.Add(this.currentPath, inputEncoding);
                }
                //var remainSeconds = remain.Seconds;
                //if (remainSeconds > 0)
                //{
                //    this.RunOnGuiThread(
                //        () => this.view.SetLogProgressCustomText(string.Format(Resources.FinishLoading, remainSeconds)));
                //}
                // Interlocked is a must because other threads can change this
                SpinWait.SpinUntil(
                    () => Interlocked.Read(ref this.queuedMessages) == 0 || this.cancellation.IsCancellationRequested);
            }
            finally
            {
                //this.RunOnGuiThread(
                //        () => this.view.SetLogProgressCustomText(Resources.LogIndexing));
                this.store.FinishAddMessages();
                reader.ProgressChanged -= this.OnReadLogProgressChanged;
                reader.CompilationStarted -= this.OnCompilationStarted;
                reader.CompilationFinished -= this.OnCompilationFinished;
                reader.EncodingDetectionStarted -= this.OnEncodingDetectionStarted;
                reader.EncodingDetectionFinished -= this.OnEncodingDetectionFinished;
            }

            this.ReadLogFromInternalStore(false);
        }

        private void OnCompilationFinished(object sender, EventArgs eventArgs)
        {
            //this.RunOnGuiThread(() => this.view.SetLogProgressCustomText(Resources.PatternCompilationFinished));
        }

        private void OnCompilationStarted(object sender, EventArgs eventArgs)
        {
            //this.RunOnGuiThread(() => this.view.SetLogProgressCustomText(Resources.PatternCompilation));
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
            //this.RunOnGuiThread(delegate
            //{
            //    this.view.SetLogProgressCustomText(string.Empty);
            //    this.view.SetFileEncoding(e.ToString());
            //});
        }

        private void OnEncodingDetectionStarted(object sender, EventArgs e)
        {
            //this.RunOnGuiThread(delegate
            //{
            //    this.view.SetLogProgressCustomText(Resources.EncodingDetectionInProgress);
            //    this.view.SetFileEncoding(string.Empty);
            //});
        }

        private void ReadLogFromInternalStore(bool signalProcess)
        {
            //this.RunOnGuiThread(() => this.view.SetLogProgressCustomText(Resources.CreateRtfInProgress));

            var rtf = string.Empty;
            Action action = delegate
            {
                try
                {
                    if (!signalProcess)
                    {
                        this.viewModel.From = this.SelectDateUsingFunc("min");
                        this.viewModel.To = this.SelectDateUsingFunc("max");
                    }
                    rtf = this.CreateRtf(signalProcess);
                }
                catch (Exception e)
                {
                    rtf = e.ToString();
                    throw;
                }
            };
            var task = Task.Factory.StartNew(action, this.cancellation.Token);

            Action successAction = () => this.RunOnGuiThread(() => this.OnSuccessRtfCreate(rtf));
            Action enableControlsAction = () => this.viewModel.UiControlsEnabled = true;

            Action<Task> onSuccess = t => this.OnComplete(t, successAction);
            Action<Task> onFailure = t => this.OnComplete(t, enableControlsAction);
            Action<Task> onCancel = t => this.OnComplete(t, enableControlsAction);
            
            task.ContinueWith(onSuccess, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
            task.ContinueWith(onFailure, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
            task.ContinueWith(onCancel, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default);
            
            this.runningTasks.Add(task, this.viewModel.LogPath);
        }

        private DateTime SelectDateUsingFunc(string func)
        {
            return this.store.SelectDateUsingFunc(func, LogLevel.Trace, LogLevel.Fatal, this.viewModel.MessageFilter, this.viewModel.UseRegularExpressions);
        }

        private void OnSuccessRtfCreate(string rtf)
        {
            this.ReadCompleted.Do(handler => handler(this, new LogReadCompletedEventArgs(rtf)));
            this.viewModel.UiControlsEnabled = true;
        }

        public void ShowElapsedTime()
        {
            //this.view.SetProgress(LoadProgress.FromPercent(100));
            this.totalReadTimeWatch.Stop();
            var text = string.Format(Resources.ReadCompletedTemplate, this.totalReadTimeWatch.Elapsed.TimespanToHumanString());
            //this.view.SetLogProgressCustomText(text);
        }

        public void ShowLogPageStatistic()
        {
            var formatTotal = ((ulong)this.TotalMessages).FormatString();
            var formatFiltered = ((ulong)this.totalFiltered).FormatString();
            var total = this.TotalMessages.ToString(formatTotal, CultureInfo.CurrentCulture);

            //this.view.LogInfo = string.Format(Resources.LogInfoFormatString,
            //    this.DisplayedMessages,
            //    total,
            //    this.CountMessages(LogLevel.Trace),
            //    this.CountMessages(LogLevel.Debug),
            //    this.CountMessages(LogLevel.Info),
            //    this.CountMessages(LogLevel.Warn),
            //    this.CountMessages(LogLevel.Error),
            //    this.CountMessages(LogLevel.Fatal),
            //    this.totalFiltered.ToString(formatFiltered, CultureInfo.CurrentCulture)
            //    );
        }

        private void OnReadLogProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //this.OnLogReadProgress(e.UserState);
        }

        public void ResetLogStatistic()
        {
            //this.view.LogInfo = string.Format(Resources.LogInfoFormatString, 0, 0, 0, 0, 0, 0, 0, 0, 0);
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
            this.ClearCache();
            this.BeginLogReading();
        }

        public void ReadRecentFiles()
        {
            IEnumerable<string> files = null;

            this.settings.UseRecentFilesStore(filesStore => files = filesStore.ReadItems());

            //this.view.ClearRecentFilesList();

            var notExistFiles = new List<string>();

            foreach (
                var item in
                    from file in files
                    where !string.IsNullOrWhiteSpace(file)
                    select file)
            {
                if (File.Exists(item))
                {
                    //this.view.CreateRecentFileItem(item);
                }
                else
                {
                    notExistFiles.Add(item);
                }
            }
            this.settings.UseRecentFilesStore(s => s.Remove(notExistFiles.ToArray()));
        }

        public void ChangeParsingTemplate(int index)
        {
            this.settings.SelectedParsingTemplate = index;
            this.SetCurrentParsingTemplate();
        }

        public void UpdateSettings(bool refresh)
        {
            var template = this.settings.ReadParsingTemplate();
            if (!template.IsEmpty)
            {
                this.CreateMessageHead(template.StartMessage, template.Compiled);
                this.CreateMessageFilter(template.Filter);
            }
            var value = this.settings.PageSize;
            if (this.pageSize != value)
            {
                this.CurrentPage = 1;
            }
            if (refresh)
            {
                this.BeginLogReading();
            }
            this.pageSize = value;
            this.SetPageSize();
        }

        public void AddCurrentFileToRecentFilesList()
        {
            this.settings.UseRecentFilesStore(s => s.Add(this.viewModel.LogPath));
        }

        public void ExportToRtf()
        {
            var path = Path.GetFileNameWithoutExtension(this.viewModel.LogPath) + ".rtf";
            //if (this.view.OpenExport(path))
            //{
            //    this.view.SaveRtf();
            //}
        }

        public int CountMessages(LogLevel level)
        {
            return this.byLevel.ContainsKey(level) ? this.byLevel[level] : 0;
        }

        public void SetPageSize()
        {
            //this.view.SetPageSize(this.pageSize);
        }

        #endregion

        #region Methods

        private long logSize;
        private long totalFiltered;

        private bool CurrentPathCached => !string.IsNullOrWhiteSpace(this.currentPath) &&
                                          this.currentPath.Equals(this.viewModel.LogPath, StringComparison.CurrentCultureIgnoreCase);

        private long TotalMessages => this.store?.CountMessages() ?? 0;

        public LogStore Store => this.store;

        private long queuedMessages;

        private string CreateRtf(bool signalProgress = false)
        {
            this.byLevel.Clear();
            var doc = new RtfDocument(Encoding.UTF8);

            if (this.store == null)
            {
                return doc.Rtf;
            }

            this.totalFiltered = this.store.CountMessages(this.viewModel.From, this.viewModel.To, (LogLevel)this.viewModel.MinLevel, (LogLevel)this.viewModel.MaxLevel, this.viewModel.MessageFilter,
                this.viewModel.UseRegularExpressions);

            if (this.CurrentPage > this.TotalPages || this.CurrentPage <= 0)
            {
                this.CurrentPage = 1;
            }

            var total = this.DisplayedMessages;
            var fraction = total / 20L;
            var signalCounter = 1;
            var count = 0;

            Action<LogMessage> onRead = delegate(LogMessage m)
            {
                this.AddMessage(doc, m);
                ++count;
                if (!signalProgress || count < signalCounter * fraction)
                {
                    return;
                }
                ++signalCounter;
                var percent = (count / (double)total) * 100;
                this.OnLogReadProgress(LoadProgress.FromPercent((int)percent));
            };

            var start = (this.CurrentPage - 1) * this.pageSize;
            this.store.ReadMessages(
                this.pageSize,
                onRead,
                () => this.NotCancelled,
                this.viewModel.From,
                this.viewModel.To,
                start,
                this.viewModel.SortingOrder == 0,
                (LogLevel)this.viewModel.MinLevel,
                (LogLevel)this.viewModel.MaxLevel,
                this.viewModel.MessageFilter,
                this.viewModel.UseRegularExpressions);
            return doc.Rtf;
        }

        private void OnLogReadProgress(object progress)
        {
            this.RunOnGuiThread(delegate
            {
                //this.view.SetProgress((LoadProgress)progress);
                var formatTotal = ((ulong)this.totalMessages).FormatString();
                var total = this.totalMessages.ToString(formatTotal, CultureInfo.CurrentCulture);
                //this.view.LogInfo = string.Format(Resources.LogInfoFormatString, 0, total, 0, 0, 0, 0, 0, 0, 0);
            });
        }

        private void AddMessage(RtfDocument doc, LogMessage message)
        {
            var logLvel = LogLevel.None;
            if (this.store.HasLogLevelProperty)
            {
                logLvel = (LogLevel)message.IntegerProperty(this.store.LogLevelProperty);
            }
            if (this.byLevel.ContainsKey(logLvel))
            {
                this.byLevel[logLvel] = this.byLevel[logLvel] + 1;
            }
            else
            {
                this.byLevel.Add(logLvel, 1);
            }

            doc.AddText(message.Header.Trim(), this.settings.FormatHead(logLvel));
            doc.AddNewLine();

            var txt = message.Body;
            if (string.IsNullOrWhiteSpace(txt))
            {
                doc.AddNewLine();
                return;
            }
            doc.AddText(txt.Trim(), this.settings.FormatBody(logLvel));
            doc.AddNewLine(3);
        }

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