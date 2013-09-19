// Created by: egr
// Created at: 19.09.2012
// © 2012-2013 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private int keepLastNFiles;

        #region Constants and Fields

        private const int DefaultPageSize = 5000;

        private readonly Dictionary<LogLevel, int> byLevel = new Dictionary<LogLevel, int>();

        private readonly List<Regex> markers;
        private Regex messageHead;
        private int pageSize;

        private readonly string settingsDatabaseFilePath;
        private CancellationTokenSource cancellation = new CancellationTokenSource();

        private string currentPath;

        private LogLevel maxFilter = LogLevel.Fatal;
        private LogLevel minFilter = LogLevel.Trace;
        private bool reverseChronological;
        private bool useRegexp = true;
        private LogStore store;
        private string textFilter;
        private ILogView view;
        private Task task;
        private int totalMessages;
        private readonly TaskScheduler uiContext;

        #endregion

        #region Constructors and Destructors

        public MainController(string startMessagePattern,
            IEnumerable<string> levels,
            string settingsDatabaseFileName,
            int keepLastNFiles,
            int pageSize = DefaultPageSize)
        {
            this.keepLastNFiles = keepLastNFiles;
            this.CurrentPage = 1;
            this.settingsDatabaseFilePath = Path.Combine(Settings.ApplicationFolder, settingsDatabaseFileName);
            this.pageSize = pageSize <= 0 ? DefaultPageSize : pageSize;
            this.markers = new List<Regex>();
            this.uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            this.CreateMarkers(levels);
            this.CreateMessageHead(startMessagePattern);
            SQLiteFunction.RegisterFunction(typeof (SqliteRegEx));
        }

        public void CreateMessageHead(string startMessagePattern)
        {
            this.messageHead = new Regex(startMessagePattern, RegexOptions.Compiled);
        }

        public void CreateMarkers(IEnumerable<string> levels)
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

        public void LoadLog(string rtf)
        {
            if (string.IsNullOrWhiteSpace(rtf))
            {
                return;
            }
            this.view.LoadLog(rtf);
            this.view.SetCurrentPage(this.CurrentPage);
            this.view.DisableBack(this.CurrentPage <= 1);
            this.view.DisableForward(this.CurrentPage >= this.TotalPages);
        }

        private void CancelPreviousTask()
        {
            try
            {
                if (this.task == null || this.task.Status != TaskStatus.Running)
                {
                    return;
                }
                if (this.cancellation.IsCancellationRequested)
                {
                    SafeRunner.Run(this.task.Wait);
                }
                else
                {
                    SafeRunner.Run(this.cancellation.Cancel);
                    SafeRunner.Run(this.task.Wait);
                    SafeRunner.Run(this.cancellation.Dispose);
                    this.cancellation = new CancellationTokenSource();
                }
            }
            finally
            {
                if (this.task != null)
                {
                    this.task.Dispose();
                }
            }
        }

        public void BeginLogReading(int min, int max, string filter, bool reverse, bool regexp)
        {
            this.CancelPreviousTask();
            this.MinFilter(min);
            this.MaxFilter(max);
            this.TextFilter(filter);
            this.UserRegexp(regexp);
            this.Ordering(reverse);
            this.view.SetProgress(0, null);

            var rtf = string.Empty;
            Func<string> function = delegate
            {
                try
                {
                    return rtf = this.ReadLog();
                }
                catch (Exception e)
                {
                    Log.Instance.Error(e.Message, e);
                    throw;
                }
            };
            this.task = Task.Factory.StartNew(function, this.cancellation.Token);
            this.task.ContinueWith(t => this.EndLogReading(rtf), this.uiContext);
        }

        private void EndLogReading(string rtf)
        {
            this.view.LogInfo = string.Format(this.view.LogInfoFormatString, this.DisplayedMessages,
                this.totalMessages, this.CountMessages(LogLevel.Trace), this.CountMessages(LogLevel.Debug),
                this.CountMessages(LogLevel.Info), this.CountMessages(LogLevel.Warn), this.CountMessages(LogLevel.Error),
                this.CountMessages(LogLevel.Fatal), this.totalFiltered);
            this.LoadLog(rtf);
            this.view.SetLoadedFileCapltion(this.view.LogPath);
            this.ReadRecentFiles();
            this.view.FocusOnTextFilterControl();
            if (string.IsNullOrWhiteSpace(rtf))
            {
                return;
            }
            this.view.SetProgress(100, null);
        }

        /// <summary>
        /// Reads log from file
        /// </summary>
        /// <returns>Path to RTF document to be loaded into control</returns>
        public string ReadLog()
        {
            if (this.minFilter > this.maxFilter && this.maxFilter >= LogLevel.Trace)
            {
                throw new ArgumentException(Resources.MinLevelGreaterThenMax);
            }
            if (!File.Exists(this.view.LogPath))
            {
                return string.Empty;
            }
            var reader = new LogReader(this.view.LogPath, this.messageHead);

            var append = reader.Length > this.logSize && this.CurrentPathCached;

            var offset = append ? this.logSize : 0L;

            this.logSize = reader.Length;

            this.RunOnGuiThread(this.SetLogSize);

            if (this.logSize == 0)
            {
                return string.Empty;
            }

            if (this.CurrentPathCached && !append)
            {
                return this.CreateRtf(true);
            }

            this.RunOnGuiThread(this.ResetLogStatistic);

            this.currentPath = reader.LogPath;


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

            try
            {
                reader.ProgressChanged += this.OnReadLogProgressChanged;
                reader.Read(this.AddMessageToCache, () => this.NotCancelled, offset);
            }
            finally
            {
                reader.ProgressChanged -= this.OnReadLogProgressChanged;
                this.store.FinishAddMessages();
            }
            return this.CreateRtf();
        }

        private void OnReadLogProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            this.OnLogReadProgress(e.ProgressPercentage, e.UserState);
        }

        private void SetLogSize()
        {
            this.view.HumanReadableLogSize = new FileSize((ulong)this.logSize).ToString();
        }

        private void ResetLogStatistic()
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

        public void PageSize(int value)
        {
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
            if (!Settings.OpenLastFile)
            {
                return;
            }
            
            var lastOpenedFile = string.Empty;

            Action<RecentFilesStore> method = delegate(RecentFilesStore filesStore)
            {
                lastOpenedFile = filesStore.ReadLastOpenedFile();
            };
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
            using (var filesStore = new RecentFilesStore(this.settingsDatabaseFilePath, this.keepLastNFiles))
            {
                action(filesStore);
            }
        }

        public void UpdateKeepLastNFiles(int value)
        {
            this.keepLastNFiles = value;
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

        private int CountMessages(LogLevel level)
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

        private long totalFiltered;
        private long logSize;

        private bool CurrentPathCached
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.currentPath) &&
                       this.currentPath.Equals(this.view.LogPath, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        private void AddMessageToCache(LogMessage message)
        {
            if (message.IsEmpty)
            {
                return;
            }
            message.Level = this.DetectLevel(message.Header);
            message.Cache();
            ++this.totalMessages;
            this.store.AddMessage(message);
        }

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
                this.OnLogReadProgress(percent, null);
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

        private void OnLogReadProgress(double percent, object speed)
        {
            this.RunOnGuiThread(() => this.view.SetProgress(percent, speed));
        }

        private void RunOnGuiThread(Action action)
        {
            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, this.uiContext);
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

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (!this.cancellation.IsCancellationRequested)
                    {
                        SafeRunner.Run(this.cancellation.Cancel);
                    }

                    if (this.task != null)
                    {
                        SafeRunner.Run(this.task.Wait);
                        SafeRunner.Run(this.task.Dispose);
                    }
                    SafeRunner.Run(this.cancellation.Dispose);
                }
                catch (Exception e)
                {
                    this.task = null;
                    this.cancellation = null;
                    Log.Instance.Fatal(e.Message, e);
                }
                finally
                {
                    if (this.store != null)
                    {
                        this.store.Dispose();
                    }
                }
            }
        }
    }
}