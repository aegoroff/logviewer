// Created by: egr
// Created at: 19.09.2012
// � 2012-2015 Alexander Egorov

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
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
using NLog;
using NLog.Targets;
using NLog.Windows.Forms;
using LogLevel = logviewer.engine.LogLevel;


namespace logviewer.core
{
    public sealed class MainController : BaseGuiController, IDisposable
    {
        private readonly ISettingsProvider settings;

        #region Constants and Fields

        private readonly Dictionary<LogLevel, int> byLevel = new Dictionary<LogLevel, int>();
        private readonly IDictionary<string, Encoding> filesEncodingCache = new ConcurrentDictionary<string, Encoding>();

        private readonly IDictionary<Task, string> runningTasks = new ConcurrentDictionary<Task, string>();

        private CancellationTokenSource cancellation = new CancellationTokenSource();
        private readonly LogCharsetDetector charsetDetector = new LogCharsetDetector();

        private string currentPath;

        private LogLevel maxFilter = LogLevel.Fatal;
        private GrokMatcher matcher;
        private GrokMatcher filter;
        private LogLevel minFilter = LogLevel.Trace;
        private int pageSize;
        private bool reverseChronological;
        private LogStore store;
        private string textFilter;
        private long totalMessages;
        private bool useRegexp = true;
        private ILogView view;
        public event EventHandler<LogReadCompletedEventArgs> ReadCompleted;
        private const int KeepLastFilters = 20;
        private const int CheckUpdatesEveryDays = 7;
        private DateTime minDate = DateTime.MinValue;
        private DateTime maxDate = DateTime.MaxValue;

        private readonly ProducerConsumerQueue queue =
            new ProducerConsumerQueue(Math.Max(2, Environment.ProcessorCount / 2));

        private readonly Stopwatch probeWatch = new Stopwatch();
        private readonly Stopwatch totalReadTimeWatch = new Stopwatch();
        private readonly TimeSpan filterUpdateDelay = TimeSpan.FromMilliseconds(200);
        private readonly RegexOptions options;
        private readonly IKernel kernel;

        #endregion

        #region Constructors and Destructors

        public MainController(ISettingsProvider settings, RegexOptions options = RegexOptions.ExplicitCapture)
        {
            this.CurrentPage = 1;
            this.settings = settings;
            this.pageSize = this.settings.PageSize;
            this.prevInput = DateTime.Now;
            this.options = options;
            this.kernel = new StandardKernel(new CoreModule());
            this.SetCurrentParsingTemplate();
            SQLiteFunction.RegisterFunction(typeof (SqliteRegEx));
        }

        private void SetCurrentParsingTemplate()
        {
            var template = this.settings.ReadParsingTemplate();
            this.CreateMessageHead(template.StartMessage, template.Compiled);
            this.CreateMessageFilter(template.Filter);
        }

        private void CreateMessageHead(string startMessagePattern, bool compiled)
        {
            this.matcher = new GrokMatcher(startMessagePattern, compiled ? options | RegexOptions.Compiled : options);
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

        private bool NotCancelled
        {
            get { return !this.cancellation.IsCancellationRequested; }
        }

        public void InitializeLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();
            LogManager.Configuration = config;
            const string layout =
                @"${date:format=yyyy-MM-dd HH\:mm\:ss,fff} ${level:upperCase=True} ${logger} ${message}${newline}${onexception:Process\: ${processname}${newline}Process time\: ${processtime}${newline}Process ID\: ${processid}${newline}Thread ID\: ${threadid}${newline}Details\:${newline}${exception:format=ToString}}";
            var boxTarget = new RichTextBoxTarget
            {
                Layout = layout,
                ControlName = "syntaxRichTextBox1",
                FormName = "MainDlg",
                UseDefaultRowColoringRules = false
            };
            boxTarget.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "White",
                FontStyle.Regular));
            boxTarget.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "White",
                FontStyle.Regular));
            boxTarget.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "DarkViolet", "White",
                FontStyle.Regular));

            var traceTarget = new TraceTarget { Layout = layout };

            config.AddTarget("box", boxTarget);
            config.AddTarget("trace", traceTarget);
            var r1 = new NLog.Config.LoggingRule("*", NLog.LogLevel.Warn, boxTarget);
            var r2 = new NLog.Config.LoggingRule("*", NLog.LogLevel.Trace, traceTarget);
            config.LoggingRules.Add(r1);
            config.LoggingRules.Add(r2);
            LogManager.Configuration = config;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void CancelPreviousTask()
        {
            if (!this.cancellation.IsCancellationRequested)
            {
                this.view.SetLogProgressCustomText(Resources.CancelPrevious);
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

        public void StartReading(string messageFilter, bool regexp)
        {
            if (!IsValidFilter(messageFilter, regexp))
            {
                return;
            }

            this.UpdateMessageFilter(messageFilter);

            this.prevInput = DateTime.Now;

            if (this.PendingStart)
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
                    this.RunOnGuiThread(() =>
                    {
                        this.UpdateRecentFilters(messageFilter);
                        this.view.StartReading();
                    });
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

        public void BeginLogReading(int min, int max, string messageFilter, bool reverse, bool regexp)
        {
            this.settings.MinLevel = min;
            this.settings.MaxLevel = max;
            this.settings.Sorting = reverse;
            this.CancelPreviousTask();
            this.MinFilter(min);
            this.MaxFilter(max);
            this.TextFilter(messageFilter);
            this.UserRegexp(regexp);
            this.Ordering(reverse);
            this.BeginLogReading();
        }

        public string CurrentEncoding
        {
            get
            {
                return this.filesEncodingCache.ContainsKey(this.currentPath)
                    ? this.filesEncodingCache[this.currentPath].EncodingName
                    : string.Empty;
            }
        }

        private void BeginLogReading()
        {
            this.RunOnGuiThread(() => this.view.SetProgress(LoadProgress.FromPercent(0)));

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

            Action<Task> onSuccess = obj => this.OnComplete(task, delegate { });
            Action<Task> onFailure = delegate
            {
                this.OnComplete(task, () => this.RunOnGuiThread(() => this.view.OnFailureRead(errorMessage)));
            };

            task.ContinueWith(onSuccess, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default);
            task.ContinueWith(onSuccess, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
            task.ContinueWith(onFailure, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
            
            this.runningTasks.Add(task, this.view.LogPath);
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

        public int ReadMinLevel()
        {
            return this.settings.MinLevel;
        }

        public int ReadMaxLevel()
        {
            return this.settings.MaxLevel;
        }

        public int ReadSorting()
        {
            return this.settings.Sorting ? 0 : 1;
        }

        public bool ReadUseRegexp()
        {
            return this.settings.UseRegexp;
        }

        public string ReadMessageFilter()
        {
            return this.settings.MessageFilter;
        }

        public void UpdateMessageFilter(string value)
        {
            this.settings.MessageFilter = value;
        }

        public void UpdateRecentFilters(string value = null)
        {
            string[] items = null;

            this.UseRecentFiltersStore(delegate(RecentItemsStore itemsStore)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    itemsStore.Add(value);
                }

                items = itemsStore.ReadItems().ToArray();
            });

            this.view.AddFilterItems(items);
        }

        public void UpdateUseRegexp(bool value)
        {
            this.settings.UseRegexp = value;
        }

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
            var checker = this.kernel.Get<UpdatesChecker>();
            this.settings.LastUpdateCheckTime = DateTime.UtcNow;
            Task.Factory.StartNew(delegate
            {
                if (!checker.IsUpdatesAvaliable())
                {
                    if (manualInvoke)
                    {
                        this.RunOnGuiThread(() => this.view.ShowNoUpdateAvaliable());
                    }
                    return;
                }

                this.RunOnGuiThread(
                    () =>
                        this.view.ShowDialogAboutNewVersionAvaliable(checker.CurrentVersion, checker.LatestVersion,
                            checker.LatestVersionUrl));
            });
        }

        /// <summary>
        ///     Reads log from file
        /// </summary>
        /// <returns>Path to RTF document to be loaded into control</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect")]
        public void StartReadLog()
        {
            totalReadTimeWatch.Restart();
            if (this.minFilter > this.maxFilter && this.maxFilter >= LogLevel.Trace)
            {
                throw new ArgumentException(Resources.MinLevelGreaterThenMax);
            }
            var reader = new LogReader(this.charsetDetector, this.matcher, this.filter);
            var logPath = this.view.LogPath;
            
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
            this.RunOnGuiThread(this.SetLogSize);
            this.RunOnGuiThread(this.ResetLogStatistic);


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
            reader.CompilationStarted += OnCompilationStarted;
            reader.CompilationFinished += this.OnCompilationFinished;
            reader.EncodingDetectionStarted += this.OnEncodingDetectionStarted;
            reader.EncodingDetectionFinished += this.OnEncodingDetectionFinished;
            Encoding inputEncoding;
            this.filesEncodingCache.TryGetValue(this.currentPath, out inputEncoding);
            try
            {
                this.queuedMessages = 0;
                var encoding = reader.Read(logPath, this.AddMessageToCache, () => this.NotCancelled, inputEncoding, offset);
                this.probeWatch.Stop();
                var elapsed = this.probeWatch.Elapsed;
                var pending = Interlocked.Read(ref this.queuedMessages);
                var inserted = this.totalMessages - pending;
                var insertRatio = inserted / elapsed.TotalSeconds;
                var remain = Math.Abs(insertRatio) < 0.00001
                    ? TimeSpan.FromSeconds(0)
                    : TimeSpan.FromSeconds(pending / insertRatio);

                if (this.currentPath != null && !this.filesEncodingCache.ContainsKey(this.currentPath) &&
                    encoding != null)
                {
                    this.filesEncodingCache.Add(this.currentPath, encoding);
                }
                var remainSeconds = remain.Seconds;
                if (remainSeconds > 0)
                {
                    this.RunOnGuiThread(
                        () => this.view.SetLogProgressCustomText(string.Format(Resources.FinishLoading, remainSeconds)));
                }
                // Interlocked is a must because other threads can change this
                SpinWait.SpinUntil(
                    () => Interlocked.Read(ref this.queuedMessages) == 0 || this.cancellation.IsCancellationRequested);
            }
            finally
            {
                this.RunOnGuiThread(
                        () => this.view.SetLogProgressCustomText(Resources.LogIndexing));
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
            this.RunOnGuiThread(() => this.view.SetLogProgressCustomText(Resources.PatternCompilationFinished));
        }

        private void OnCompilationStarted(object sender, EventArgs eventArgs)
        {
            this.RunOnGuiThread(() => this.view.SetLogProgressCustomText(Resources.PatternCompilation));
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
            this.RunOnGuiThread(delegate
            {
                this.view.SetLogProgressCustomText(string.Empty);
                this.view.SetFileEncoding(e.ToString());
            });
        }

        private void OnEncodingDetectionStarted(object sender, EventArgs e)
        {
            this.RunOnGuiThread(delegate
            {
                this.view.SetLogProgressCustomText(Resources.EncodingDetectionInProgress);
                this.view.SetFileEncoding(string.Empty);
            });
        }

        private void ReadLogFromInternalStore(bool signalProcess)
        {
            this.RunOnGuiThread(() => this.view.SetLogProgressCustomText(Resources.CreateRtfInProgress));

            var rtf = string.Empty;
            Action action = delegate
            {
                try
                {
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
            Action failAction = () => this.RunOnGuiThread(() => this.view.OnFailureRead(rtf));

            Action<Task> onSuccess = t => this.OnComplete(t, successAction);
            Action<Task> onFailure = t => this.OnComplete(t, failAction);
            Action<Task> onCancel = t => this.OnComplete(t, () => {});
            
            task.ContinueWith(onSuccess, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
            task.ContinueWith(onFailure, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
            task.ContinueWith(onCancel, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Default);
            
            this.runningTasks.Add(task, this.view.LogPath);
        }

        private void OnSuccessRtfCreate(string rtf)
        {
            this.ReadCompleted.Do(handler => handler(this, new LogReadCompletedEventArgs(rtf)));
        }

        public void ShowElapsedTime()
        {
            this.view.SetProgress(LoadProgress.FromPercent(100));
            this.totalReadTimeWatch.Stop();
            var text = string.Format(Resources.ReadCompletedTemplate, this.totalReadTimeWatch.Elapsed.TimespanToHumanString());
            this.view.SetLogProgressCustomText(text);
        }

        public void ShowLogPageStatistic()
        {
            var formatTotal = ((ulong)this.TotalMessages).FormatString();
            var formatFiltered = ((ulong)this.totalFiltered).FormatString();
            var total = this.TotalMessages.ToString(formatTotal, CultureInfo.CurrentCulture);

            this.view.LogInfo = string.Format(Resources.LogInfoFormatString,
                this.DisplayedMessages,
                total,
                this.CountMessages(LogLevel.Trace),
                this.CountMessages(LogLevel.Debug),
                this.CountMessages(LogLevel.Info),
                this.CountMessages(LogLevel.Warn),
                this.CountMessages(LogLevel.Error),
                this.CountMessages(LogLevel.Fatal),
                this.totalFiltered.ToString(formatFiltered, CultureInfo.CurrentCulture)
                );
        }

        private void OnReadLogProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            this.OnLogReadProgress(e.UserState);
        }

        private void SetLogSize()
        {
            this.view.SetLoadedFileCapltion(this.currentPath);
            this.view.HumanReadableLogSize = new FileSize(this.logSize, true).Format();
        }

        public void ResetLogStatistic()
        {
            this.view.LogInfo = string.Format(Resources.LogInfoFormatString, 0, 0, 0, 0, 0, 0, 0, 0, 0);
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

        public void MinFilter(int value)
        {
            this.minFilter = (LogLevel)value;
        }

        public void MaxFilter(int value)
        {
            this.maxFilter = (LogLevel)value;
        }
        
        public void MinDate(DateTime value)
        {
            this.minDate = value;
        }

        public void MaxDate(DateTime value)
        {
            this.maxDate = value;
        }

        public void TextFilter(string value)
        {
            this.textFilter = string.IsNullOrWhiteSpace(value)
                ? null
                : value;
        }

        public void UserRegexp(bool enabled)
        {
            this.useRegexp = enabled;
        }

        private void Ordering(bool reverse)
        {
            this.reverseChronological = reverse;
        }

        public void LoadLastOpenedFile()
        {
            if (!this.settings.OpenLastFile)
            {
                return;
            }

            var lastOpenedFile = string.Empty;

            Action<RecentItemsStore> method =
                delegate(RecentItemsStore filesStore) { lastOpenedFile = filesStore.ReadLastUsedItem(); };
            this.UseRecentFilesStore(method);

            if (!string.IsNullOrWhiteSpace(lastOpenedFile))
            {
                this.view.StartLoadingLog(lastOpenedFile);
            }
        }

        public void ReadRecentFiles()
        {
            IEnumerable<string> files = null;

            this.UseRecentFilesStore(delegate(RecentItemsStore filesStore) { files = filesStore.ReadItems(); });

            this.view.ClearRecentFilesList();

            var notExistFiles = new List<string>();

            foreach (
                var item in
                    from file in files
                    where !string.IsNullOrWhiteSpace(file)
                    select file)
            {
                if (File.Exists(item))
                {
                    this.view.CreateRecentFileItem(item);
                }
                else
                {
                    notExistFiles.Add(item);
                }
            }
            this.UseRecentFilesStore(s => s.Remove(notExistFiles.ToArray()));
        }

        public void ReadTemplates()
        {
            this.view.ClearTemplatesList();
            Task.Factory.StartNew(delegate
            {
                var templates = settings.ReadAllParsingTemplates();
                var current = settings.SelectedParsingTemplate;
                foreach (var template in templates)
                {
                    var t = template;
                    this.RunOnGuiThread(() => this.view.CreateTemplateSelectionItem(t, current));
                }
            });
        }

        public void ChangeParsingTemplate(int index)
        {
            this.settings.SelectedParsingTemplate = index;
            this.SetCurrentParsingTemplate();
        }

        private void UseRecentFilesStore(Action<RecentItemsStore> action)
        {
            UseRecentFiltersStore(action, "RecentFiles");
        }
        
        private void UseRecentFiltersStore(Action<RecentItemsStore> action)
        {
            UseRecentFiltersStore(action, "RecentFilters", KeepLastFilters);
        }
        
        private void UseRecentFiltersStore(Action<RecentItemsStore> action, string table, int maxItems = 0)
        {
            try
            {
                using (var itemsStore = new RecentItemsStore(this.settings, table, maxItems))
                {
                    action(itemsStore);
                }
            }
            catch (Exception e)
            {
                Log.Instance.Debug(e);
            }
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
                this.BeginLogReading((int)this.minFilter, (int)this.maxFilter, this.textFilter, this.reverseChronological, this.useRegexp);
            }
            this.pageSize = value;
            this.SetPageSize();
        }

        public void AddCurrentFileToRecentFilesList()
        {
            this.UseRecentFilesStore(s => s.Add(this.view.LogPath));
        }

        public void OpenLogFile()
        {
            if (!this.view.OpenLogFile())
            {
                return;
            }
            this.ClearCache();
            this.CurrentPage = 1;
            this.view.LogPath = this.view.LogFileName;
            this.view.ReadLog();
        }

        public void ExportToRtf()
        {
            var path = Path.GetFileNameWithoutExtension(this.view.LogPath) + ".rtf";
            if (this.view.OpenExport(path))
            {
                this.view.SaveRtf();
            }
        }

        public int CountMessages(LogLevel level)
        {
            return this.byLevel.ContainsKey(level) ? this.byLevel[level] : 0;
        }

        public void SetView(ILogView logView)
        {
            this.view = logView;
            this.view.Initialize();
        }

        public void SetPageSize()
        {
            this.view.SetPageSize(this.pageSize);
        }

        #endregion

        #region Methods

        private long logSize;
        private long totalFiltered;

        private bool CurrentPathCached
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.currentPath) &&
                       this.currentPath.Equals(this.view.LogPath, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        private long TotalMessages
        {
            get { return this.store != null ? this.store.CountMessages() : 0; }
        }

        public LogStore Store
        {
            get { return this.store; }
        }

        private long queuedMessages;

        private string CreateRtf(bool signalProgress = false)
        {
            this.byLevel.Clear();
            var doc = new RtfDocument(Encoding.UTF8);

            if (this.store == null)
            {
                return doc.Rtf;
            }

            this.totalFiltered = this.store.CountMessages(this.minDate, this.maxDate, this.minFilter, this.maxFilter, this.textFilter,
                this.useRegexp);

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
                this.minDate,
                this.maxDate,
                start,
                this.reverseChronological,
                this.minFilter,
                this.maxFilter,
                this.textFilter,
                this.useRegexp);
            return doc.Rtf;
        }

        private void OnLogReadProgress(object progress)
        {
            this.RunOnGuiThread(delegate
            {
                this.view.SetProgress((LoadProgress)progress);
                var formatTotal = ((ulong)this.totalMessages).FormatString();
                var total = this.totalMessages.ToString(formatTotal, CultureInfo.CurrentCulture);
                this.view.LogInfo = string.Format(Resources.LogInfoFormatString, 0, total, 0, 0, 0, 0, 0, 0, 0);
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