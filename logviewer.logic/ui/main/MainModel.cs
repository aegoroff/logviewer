// Created by: egr
// Created at: 07.11.2015
// © 2012-2016 Alexander Egorov

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Humanizer;
using logviewer.engine;
using logviewer.logic.Annotations;
using logviewer.logic.models;
using logviewer.logic.Properties;
using logviewer.logic.storage;
using logviewer.logic.support;
using LogLevel = logviewer.engine.LogLevel;


namespace logviewer.logic.ui.main
{
    public sealed class MainModel : UiSynchronizeModel, IDisposable
    {
        private readonly ISettingsProvider settings;

        #region Constants and Fields

        private readonly Dictionary<LogLevel, ulong> byLevel = new Dictionary<LogLevel, ulong>();
        private readonly IDictionary<string, Encoding> filesEncodingCache = new ConcurrentDictionary<string, Encoding>();

        private CancellationTokenSource cancellation = new CancellationTokenSource();
        private readonly LogCharsetDetector charsetDetector = new LogCharsetDetector();
        private LogReader reader;

        private string currentPath;

        private MessageMatcher matcher;
        private LogStore store;
        private long totalMessages;
        private readonly IMainViewModel viewModel;

        private readonly ProducerConsumerMessageQueue queue =
            new ProducerConsumerMessageQueue(Math.Max(2, Environment.ProcessorCount / 2));

        private readonly Stopwatch probeWatch = new Stopwatch();
        private readonly Stopwatch totalReadTimeWatch = new Stopwatch();
        private readonly TimeSpan filterUpdateDelay = TimeSpan.FromMilliseconds(200);
        public event EventHandler<EventArgs> ReadCompleted;
        private const int CheckUpdatesEveryDays = 7;
        private bool readCompleted = true;
        private const int WaitCancelSeconds = 5;
        private const int LogChangedThrottleIntervalMilliseconds = 500;
        private IObserver<string> logChangedObserver;
        private IObserver<string> filterChangedObserver;

        #endregion

        #region Constructors and Destructors

        public MainModel(IMainViewModel viewModel)
        {
            this.settings = viewModel.SettingsProvider;
            this.viewModel = viewModel;
            this.VersionsReader = new VersionsReader(this.viewModel.GithubAccount, this.viewModel.GithubProject);
            viewModel.PropertyChanged += this.ViewModelOnPropertyChanged;

            var logChangedObservable = Observable.Create<string>(observer =>
            {
                this.logChangedObserver = observer;
                return Disposable.Empty;
            });

            var filterChangedObservable = Observable.Create<string>(observer =>
            {
                this.filterChangedObserver = observer;
                return Disposable.Empty;
            });

            logChangedObservable.SubscribeOn(Scheduler.Default)
                .Throttle(TimeSpan.FromMilliseconds(LogChangedThrottleIntervalMilliseconds))
                .Subscribe(this.OnChangeLog);

            filterChangedObservable.SubscribeOn(Scheduler.Default)
                .Throttle(this.filterUpdateDelay)
                .Subscribe(this.StartReadingLogOnFilterChange);
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
                case nameof(this.viewModel.SortingOrder):
                    this.StartReadingLogOnFilterChange();
                    break;
                case nameof(this.viewModel.UseRegularExpressions):
                    if (!string.IsNullOrWhiteSpace(this.viewModel.MessageFilter))
                    {
                        this.StartReadingLogOnFilterChange();
                    }
                    break;
                case nameof(this.viewModel.MessageFilter):
                    this.viewModel.IsTextFilterFocused = false;
                    this.StartReadingLogOnFilterChange();
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

            var observable = Observable.Return(checker, Scheduler.Default);

            this.settings.LastUpdateCheckTime = DateTime.UtcNow;

            observable.Subscribe(
                c =>
                {
                    c.CheckUpdatesAvaliable(result =>
                    {
                        if (!result)
                        {
                            if (manualInvoke)
                            {
                                this.RunOnGuiThread(() => this.viewModel.ShowNoUpdateAvaliable());
                            }
                            return;
                        }
                        this.RunOnGuiThread(() => this.viewModel.ShowDialogAboutNewVersionAvaliable(c.CurrentVersion, c.LatestVersion, c.LatestVersionUrl));
                    });
                });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void CancelPreviousRead()
        {
            if (this.reader == null)
            {
                return;
            }

            if (!this.cancellation.IsCancellationRequested)
            {
                this.reader.Cancel();
                this.viewModel.LogProgressText = Resources.CancelPrevious;
                SafeRunner.Run(this.cancellation.Cancel);
            }
            this.queue.CleanupPendingTasks();

            SpinWait.SpinUntil(() => this.readCompleted, TimeSpan.FromSeconds(WaitCancelSeconds));

            SafeRunner.Run(this.cancellation.Dispose);
        }

        public void StartReadingLogOnFilterChange()
        {
            if (!this.viewModel.UiControlsEnabled)
            {
                return;
            }

            this.filterChangedObserver.OnNext(this.viewModel.MessageFilter);
        }

        private void StartReadingLogOnFilterChange(string filter)
        {
            if (!filter.IsValid(this.viewModel.UseRegularExpressions))
            {
                return;
            }

            this.UpdateRecentFilters(filter);
            this.StartLogReadingTask();
        }


        private void StartLogReadingTask()
        {
            this.cancellation = new CancellationTokenSource();

            var o = Observable.Start(this.DoLogReadingTask, Scheduler.Default);
            o.ObserveOn(this.UiContextScheduler)
                .Finally(this.FinalizeLogReadingTask)
                .Subscribe(unit => { },
                    exception =>
                    {
                        this.viewModel.LogProgressText = exception.Message;
                        Log.Instance.Warn(exception.Message, exception);
                    },
                    () => this.viewModel.Datasource.LoadCount((int) this.viewModel.MessageCount), this.cancellation.Token);
        }

        private void FinalizeLogReadingTask()
        {
            this.viewModel.UiControlsEnabled = true;
            this.viewModel.IsTextFilterFocused = true;
        }

        public void UpdateLog(string path)
        {
            if (string.IsNullOrWhiteSpace(path) ||
                !path.Equals(this.currentPath, StringComparison.CurrentCultureIgnoreCase) || !this.settings.AutoRefreshOnFileChange)
            {
                return;
            }
            this.logChangedObserver.OnNext(path);
        }

        private void OnChangeLog(string s)
        {
            try
            {
                var f = new FileInfo(s);
                if (f.Length < this.logSize)
                {
                    this.currentPath = string.Empty;
                    this.logSize = 0;
                    this.store?.Dispose();
                    this.store = null;
                }
                if (f.Length == this.logSize)
                {
                    return;
                }
                this.StartLogReadingTask();
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message, e);
            }
        }

        private void UpdateRecentFilters(string value = null)
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
                this.AfterDatabaseCreation(false);
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

            this.InitNewStoreIfNecessary(append);

            GC.Collect();
            this.store.StartAddMessages();
            this.readCompleted = false;

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
                this.queue.ResetQueuedCount();

                this.RunReading(logPath, inputEncoding, offset);

                this.probeWatch.Stop();

                this.FinishLoading();

                SpinWait.SpinUntil(
                    () => this.queue.ReadCompleted || this.reader.Cancelled);
            }
            finally
            {
                this.reader.ProgressChanged -= this.OnReadLogProgressChanged;
                this.reader.CompilationStarted -= this.OnCompilationStarted;
                this.reader.CompilationFinished -= this.OnCompilationFinished;
                this.reader.EncodingDetectionStarted -= this.OnEncodingDetectionStarted;
                this.reader.EncodingDetectionFinished -= this.OnEncodingDetectionFinished;

                this.viewModel.LogProgressText = Resources.LogIndexing;
                try
                {
                    if (!this.reader.Cancelled)
                    {
                        this.store.FinishAddMessages();
                    }
                }
                finally
                {
                    this.readCompleted = true;
                }
            }
            this.UpdateStatisticByLevel();
            this.AfterDatabaseCreation(false);
        }

        private void InitNewStoreIfNecessary(bool append)
        {
            if (this.store != null && !append)
            {
                this.store.Dispose();
            }
            if (append && this.store != null)
            {
                return;
            }
            var dbSize = this.logSize + (this.logSize / 10) * 4; // +40% to log file
            this.store = new LogStore(dbSize, null, this.matcher.IncludeMatcher.MessageSchema);
            this.queue.Store = this.store;
        }

        private void RunReading(string logPath, Encoding inputEncoding, long offset)
        {
            foreach (var message in this.reader.Read(logPath, inputEncoding, offset))
            {
                this.AddMessageToCache(message);
            }
        }

        private void UpdateStatisticByLevel()
        {
            for (var i = 0; i < (int) LogLevel.Fatal + 1; i++)
            {
                var level = (LogLevel) i;
                this.byLevel[level] =
                    (ulong) this.store.CountMessages(level, level, this.viewModel.MessageFilter, this.viewModel.UseRegularExpressions, true);
            }
        }

        private void FinishLoading()
        {
            var elapsed = this.probeWatch.Elapsed;
            var pending = this.queue.QueuedMessages;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddMessageToCache(LogMessage message)
        {
            if (message.IsEmpty || this.cancellation.IsCancellationRequested)
            {
                return;
            }
            this.queue.IncrementQueuedCount();
            message.Ix = Interlocked.Increment(ref this.totalMessages);
            this.queue.EnqueueItem(message);
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

            this.viewModel.Provider.FilterViewModel = new MessageFilterViewModel
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

            this.viewModel.MessageCount = this.viewModel.Provider.FetchCount();
            this.viewModel.ToDisplayMessages = this.viewModel.MessageCount.ToString("N0", CultureInfo.CurrentCulture); // Not L10N
            this.viewModel.LogStatistic = string.Format(Resources.LoStatisticFormatString,
                this.CountMessages(LogLevel.Trace),
                this.CountMessages(LogLevel.Debug),
                this.CountMessages(LogLevel.Info),
                this.CountMessages(LogLevel.Warn),
                this.CountMessages(LogLevel.Error),
                this.CountMessages(LogLevel.Fatal)
                );

            this.ReadCompleted?.Invoke(this, new EventArgs());
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
            this.CancelPreviousRead();
            this.ClearCache();
            this.UpdateSettings(true);
        }

        public void AddCurrentFileToRecentFilesList()
        {
            this.settings.UseRecentFilesStore(s => s.Add(this.viewModel.LogPath));
        }

        [PublicAPI]
        public string CountMessages(LogLevel level)
        {
            return ToHumanReadableString(this.byLevel.ContainsKey(level) ? this.byLevel[level] : 0);
        }

        private long logSize;

        private bool CurrentPathCached => !string.IsNullOrWhiteSpace(this.currentPath) &&
                                          this.currentPath.Equals(this.viewModel.LogPath, StringComparison.CurrentCultureIgnoreCase);

        private long TotalMessages => this.store?.CountMessages() ?? 0;

        public LogStore Store => this.store;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed",
            MessageId = "cancellation"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed",
             MessageId = "store")]
        public void Dispose()
        {
            this.VersionsReader.Dispose();
            this.queue.Shutdown(true);
            try
            {
                SafeRunner.Run(this.CancelPreviousRead);
            }
            finally
            {
                SafeRunner.Run(() => this.store?.Dispose());
            }
        }
    }
}