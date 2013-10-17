// Created by: egr
// Created at: 19.09.2012
// © 2012-2013 Alexander Egorov

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using logviewer.core.Properties;
using Net.Sgoliver.NRtfTree.Util;
using NLog.Targets;

namespace logviewer.core
{
    public sealed class MainController : IDisposable
    {
        private readonly ISettingsProvider settings;

        #region Constants and Fields

        private readonly Dictionary<LogLevel, int> byLevel = new Dictionary<LogLevel, int>();
        private readonly IDictionary<string, Encoding> filesEncodingCache = new ConcurrentDictionary<string, Encoding>();

        private readonly List<Regex> markers;
        private readonly IDictionary<Task, string> runningTasks = new ConcurrentDictionary<Task, string>();
        private readonly TaskScheduler uiContext;
        private readonly SynchronizationContext winformsOrDefaultContext;

        private CancellationTokenSource cancellation = new CancellationTokenSource();

        private string currentPath;

        private LogLevel maxFilter = LogLevel.Fatal;
        private Regex messageHead;
        private LogLevel minFilter = LogLevel.Trace;
        private int pageSize;
        private bool reverseChronological;
        private LogStore store;
        private string textFilter;
        private long totalMessages;
        private bool useRegexp = true;
        private ILogView view;
        public event EventHandler<LogReadCompletedEventArgs> ReadCompleted;

        private readonly ProducerConsumerQueue queue =
            new ProducerConsumerQueue(Math.Max(2, Environment.ProcessorCount / 2));

        private readonly Stopwatch stopWatch = new Stopwatch();

        #endregion

        #region Constructors and Destructors

        public MainController(ISettingsProvider settings)
        {
            this.CurrentPage = 1;
            this.settings = settings;
            this.pageSize = this.settings.PageSize;
            this.markers = new List<Regex>();
            this.uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            this.winformsOrDefaultContext = SynchronizationContext.Current ?? new SynchronizationContext();
            var template = this.settings.ReadParsingTemplate();
            this.CreateMarkers(template.Levels);
            this.CreateMessageHead(template.StartMessage);
            SQLiteFunction.RegisterFunction(typeof (SqliteRegEx));
        }

        private void CreateMessageHead(string startMessagePattern)
        {
            this.messageHead = new Regex(startMessagePattern, RegexOptions.Compiled);
        }

        private void CreateMarkers(IEnumerable<string> levels)
        {
            this.markers.Clear();
            this.markers.AddRange(levels.Select(level => level.ToMarker()));
        }

        #endregion

        #region Public Properties

        public long MessagesCount
        {
            get
            {
                var start = (this.CurrentPage - 1) * this.pageSize;
                return this.TotalFiltered - start;
            }
        }

        public int CurrentPage { get; set; }

        public int TotalPages
        {
            get
            {
                if (this.TotalFiltered == 0)
                {
                    return 1;
                }
                return (int)Math.Ceiling(this.TotalFiltered / (float)this.pageSize);
            }
        }

        public long DisplayedMessages
        {
            get
            {
                var finish = Math.Min(this.MessagesCount, this.pageSize);
                return Math.Min(finish, this.TotalFiltered);
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
            var target = new RichTextBoxTarget
            {
                Layout =
                    @"${date:format=yyyy-MM-dd HH\:mm\:ss,fff} ${level:upperCase=True} ${logger} ${message}${newline}${onexception:Process\: ${processname}${newline}Process time\: ${processtime}${newline}Process ID\: ${processid}${newline}Thread ID\: ${threadid}${newline}Details\:${newline}${exception:format=ToString}}",
                ControlName = "syntaxRichTextBox1",
                FormName = "MainDlg",
                UseDefaultRowColoringRules = false
            };
            target.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "White",
                FontStyle.Regular));
            target.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "White",
                FontStyle.Regular));
            target.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "DarkViolet", "White",
                FontStyle.Regular));
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, NLog.LogLevel.Warn);
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

        public void BeginLogReading(int min, int max, string filter, bool reverse, bool regexp)
        {
            this.settings.MinLevel = min;
            this.settings.MaxLevel = max;
            this.settings.Sorting = reverse;
            this.CancelPreviousTask();
            this.MinFilter(min);
            this.MaxFilter(max);
            this.TextFilter(filter);
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
            var task = Task.Factory.StartNew(action, this.cancellation.Token, TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            task.ContinueWith(t => this.view.OnFailureRead(errorMessage), CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted, this.uiContext);
            this.runningTasks.Add(task, this.view.LogPath);
        }

        public void UpdateLog(string path)
        {
            if (string.IsNullOrWhiteSpace(path) ||
                !path.Equals(this.currentPath, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }
            Action action = delegate
            {
                this.WaitRunningTasks();
                var f = new FileInfo(path);
                if (f.Length > this.logSize)
                {
                    this.BeginLogReading();
                }
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

        public void UpdateUseRegexp(bool value)
        {
            this.settings.UseRegexp = value;
        }

        /// <summary>
        ///     Reads log from file
        /// </summary>
        /// <returns>Path to RTF document to be loaded into control</returns>
        public void StartReadLog()
        {
            if (this.minFilter > this.maxFilter && this.maxFilter >= LogLevel.Trace)
            {
                throw new ArgumentException(Resources.MinLevelGreaterThenMax);
            }
            var reader = new LogReader(this.view.LogPath, this.messageHead);

            var append = reader.Length > this.logSize && this.CurrentPathCached;

            var offset = append ? this.logSize : 0L;

            this.logSize = reader.Length;

            if (this.logSize == 0)
            {
                throw new ArgumentException(Resources.ZeroFileDetected);
            }

            if (this.CurrentPathCached && !append)
            {
                this.ReadLogFromInternalStore(true);
                return;
            }

            this.currentPath = reader.LogPath;
            this.RunOnGuiThread(this.SetLogSize);
            this.RunOnGuiThread(this.ResetLogStatistic);


            var dbSize = this.logSize + (this.logSize / 10) * 4; // +40% to log file
            if (this.store != null && !append)
            {
                this.store.Dispose();
            }
            if (!append || this.store == null)
            {
                this.store = new LogStore(dbSize);
            }
            GC.Collect();
            this.store.StartAddMessages();
            if (!append)
            {
                this.totalMessages = 0;
            }
            reader.ProgressChanged += this.OnReadLogProgressChanged;
            reader.EncodingDetectionStarted += this.OnEncodingDetectionStarted;
            reader.EncodingDetectionFinished += this.OnEncodingDetectionFinished;
            Encoding inputEncoding;
            this.filesEncodingCache.TryGetValue(this.currentPath, out inputEncoding);
            try
            {
                this.queuedMessages = 0;
                var encoding = reader.Read(this.AddMessageToCache, () => this.NotCancelled, inputEncoding, offset);
                this.stopWatch.Stop();
                var elapsed = this.stopWatch.Elapsed;
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
                this.store.FinishAddMessages();
                reader.ProgressChanged -= this.OnReadLogProgressChanged;
                reader.EncodingDetectionStarted -= this.OnEncodingDetectionStarted;
                reader.EncodingDetectionFinished -= this.OnEncodingDetectionFinished;
            }

            this.ReadLogFromInternalStore(false);
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
            message.Ix = ++this.totalMessages;

            this.queue.EnqueueItem(delegate
            {
                try
                {
                    message.Level = this.DetectLevel(message.Header);
                    message.Cache();
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
            this.stopWatch.Restart();
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
            task.ContinueWith(t => this.OnSuccessRtfCreate(rtf), CancellationToken.None,
                TaskContinuationOptions.OnlyOnRanToCompletion,
                this.uiContext);
            task.ContinueWith(t => this.view.OnFailureRead(rtf), CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                this.uiContext);
            this.runningTasks.Add(task, this.view.LogPath);
        }

        private void OnSuccessRtfCreate(string rtf)
        {
            if (this.ReadCompleted != null)
            {
                this.ReadCompleted(this, new LogReadCompletedEventArgs(rtf));
            }
        }

        private void OnReadLogProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            this.OnLogReadProgress(e.UserState);
        }

        private void SetLogSize()
        {
            this.view.SetLoadedFileCapltion(this.currentPath);
            this.view.HumanReadableLogSize = new FileSize((ulong)this.logSize).ToString();
        }

        public void ResetLogStatistic()
        {
            this.view.LogInfo = string.Format(this.view.LogInfoFormatString, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        public void CancelReading()
        {
            if (this.cancellation.IsCancellationRequested)
            {
                return;
            }
            this.cancellation.Cancel();
        }

        public void ClearCache()
        {
            this.currentPath = null;
        }

        public void MinFilter(int value)
        {
            this.minFilter = (LogLevel)value;
        }

        public void MaxFilter(int value)
        {
            this.maxFilter = (LogLevel)value;
        }

        private void PageSize()
        {
            var value = this.settings.PageSize;
            if (this.pageSize != value)
            {
                this.CurrentPage = 1;
                this.BeginLogReading((int)this.minFilter, (int)this.maxFilter, this.textFilter,
                    this.reverseChronological, this.useRegexp);
            }
            this.pageSize = value;
            this.SetPageSize();
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

            Action<RecentFilesStore> method =
                delegate(RecentFilesStore filesStore) { lastOpenedFile = filesStore.ReadLastOpenedFile(); };
            this.UseRecentFilesStore(method);

            if (!string.IsNullOrWhiteSpace(lastOpenedFile))
            {
                this.view.StartLoadingLog(lastOpenedFile);
            }
        }

        public void ReadRecentFiles()
        {
            IEnumerable<string> files = null;

            this.UseRecentFilesStore(delegate(RecentFilesStore filesStore) { files = filesStore.ReadFiles(); });

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

        private void UseRecentFilesStore(Action<RecentFilesStore> action)
        {
            using (var filesStore = new RecentFilesStore(this.settings))
            {
                action(filesStore);
            }
        }

        public void UpdateSettings()
        {
            var template = this.settings.ReadParsingTemplate();
            if (!template.IsEmpty)
            {
                this.CreateMarkers(template.Levels);
                this.CreateMessageHead(template.StartMessage);
            }
            this.PageSize();
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

        public long TotalMessages
        {
            get { return this.store != null ? this.store.CountMessages() : 0; }
        }

        public long TotalFiltered
        {
            get { return this.totalFiltered; }
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

            this.totalFiltered = this.store.CountMessages(this.minFilter, this.maxFilter, this.textFilter,
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
                this.view.LogInfo = string.Format(this.view.LogInfoFormatString, 0, this.totalMessages, 0, 0, 0, 0, 0, 0, 0);
            });
        }

        private void RunOnGuiThread(Action action)
        {
            this.winformsOrDefaultContext.Post(o => action(), null);
        }

        private void AddMessage(RtfDocument doc, LogMessage message)
        {
            if (this.byLevel.ContainsKey(message.Level))
            {
                this.byLevel[message.Level] = this.byLevel[message.Level] + 1;
            }
            else
            {
                this.byLevel.Add(message.Level, 1);
            }

            doc.AddText(message.Header.Trim(), message.HeadFormat);
            doc.AddNewLine();

            var txt = message.Body;
            if (string.IsNullOrWhiteSpace(txt))
            {
                doc.AddNewLine();
                return;
            }
            doc.AddText(txt.Trim(), message.BodyFormat);
            doc.AddNewLine(3);
        }

        private LogLevel DetectLevel(string line, LogLevel defaultLevel = LogLevel.Trace)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return defaultLevel;
            }
            for (var i = 0; i < this.markers.Count; i++)
            {
                if (this.markers[i].IsMatch(line))
                {
                    return (LogLevel)i;
                }
            }

            return defaultLevel;
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
                if (this.store != null)
                {
                    SafeRunner.Run(this.store.Dispose);
                }
            }
        }
    }
}