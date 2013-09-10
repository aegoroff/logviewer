using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
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
        #region Constants and Fields

        private const string NewLine = "\n";
        private const int DefaultPageSize = 10000;

        private readonly Dictionary<LogLevel, int> byLevel = new Dictionary<LogLevel, int>();

        private readonly List<Regex> markers;
        private readonly Regex messageHead;
        private readonly int pageSize;

        private readonly List<string> recentFiles = new List<string>();
        private readonly string recentFilesFilePath;
        private CancellationTokenSource cancellation = new CancellationTokenSource();

        private string currentPath;

        private LogLevel maxFilter = LogLevel.Fatal;
        private LogLevel minFilter = LogLevel.Trace;
        private bool reverseChronological;
        private LogStore store;
        private string textFilter;
        private ILogView view;
        private Task task;
        private int totalMessages;

        #endregion

        #region Constructors and Destructors

        public MainController(string startMessagePattern,
            string recentFilesFilePath,
            IEnumerable<string> levels,
            int pageSize = DefaultPageSize)
        {
            this.CurrentPage = 1;
            this.recentFilesFilePath = recentFilesFilePath;
            this.pageSize = pageSize <= 0 ? DefaultPageSize : pageSize;
            this.markers = new List<Regex>();
            this.markers.AddRange(levels.Select(level => level.ToMarker()));
            this.messageHead = new Regex(startMessagePattern, RegexOptions.Compiled);
            SQLiteFunction.RegisterFunction(typeof(SqliteRegEx));
        }

        #endregion

        #region Public Properties

        public long LogSize { get; private set; }

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

        public void LoadLog(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }
            try
            {
                this.view.LoadLog(path);
                this.view.SetCurrentPage(this.CurrentPage);
                this.view.DisableBack(this.CurrentPage <= 1);
                this.view.DisableForward(this.CurrentPage >= this.TotalPages);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private void CancelPreviousTask()
        {
            if (this.task == null || this.task.Status != TaskStatus.Running)
            {
                return;
            }
            if (this.cancellation.IsCancellationRequested)
            {
                this.task.Wait();
            }
            else
            {
                this.cancellation.Cancel();
                this.task.Wait();
                this.cancellation.Dispose();
                this.cancellation = new CancellationTokenSource();
            }
        }

        public void BeginLogReading(int min, int max, string filter, bool reverse)
        {
            this.CancelPreviousTask();
            this.MinFilter(min);
            this.MaxFilter(max);
            this.TextFilter(filter);
            this.Ordering(reverse);
            var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
            var path = string.Empty;
            Func<string> function = delegate
            {
                try
                {
                    return path = this.ReadLog();
                }
                catch (Exception e)
                {
                    Log.Instance.Error(e.Message, e);
                    throw;
                }
            };
            this.task = Task.Factory.StartNew(function, this.cancellation.Token);
            this.task.ContinueWith(t => this.EndLogReading(path), uiContext);
        }

        private void EndLogReading(string path)
        {
            this.view.HumanReadableLogSize = new FileSize((ulong)this.LogSize).ToString();
            this.view.LogInfo = string.Format(this.view.LogInfoFormatString, this.DisplayedMessages,
                this.totalMessages, this.CountMessages(LogLevel.Trace), this.CountMessages(LogLevel.Debug),
                this.CountMessages(LogLevel.Info), this.CountMessages(LogLevel.Warn), this.CountMessages(LogLevel.Error),
                this.CountMessages(LogLevel.Fatal), this.totalFiltered);
            this.LoadLog(path);
            this.view.SetLoadedFileCapltion(this.view.LogPath);
            this.ReadRecentFiles();
            this.view.FocusOnTextFilterControl();
        }

        public string ReadLog()
        {
            if (this.minFilter > this.maxFilter && this.maxFilter >= LogLevel.Trace)
            {
                throw new ArgumentException(Resources.MinLevelGreaterThenMax);
            }
            var convertedPath = this.Convert();
            var useConverted = !string.IsNullOrWhiteSpace(convertedPath) &&
                               !convertedPath.Equals(this.view.LogPath, StringComparison.OrdinalIgnoreCase);
            var filePath = useConverted ? convertedPath : this.view.LogPath;
            try
            {
                if (!File.Exists(this.view.LogPath))
                {
                    return string.Empty;
                }
                var fi = new FileInfo(filePath);
                this.LogSize = fi.Length;
                if (fi.Length == 0)
                {
                    return string.Empty;
                }
                if (this.CurrentPathCached)
                {
                    return this.CreateRtf();
                }
                this.currentPath = this.view.LogPath;

                using (
                    var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, Guid.NewGuid().ToString(), 0,
                        MemoryMappedFileAccess.Read))
                {
                    using (var mmStream = mmf.CreateViewStream(0, fi.Length, MemoryMappedFileAccess.Read))
                    {
                        return this.ReadLog(mmStream);
                    }
                }
            }
            finally
            {
                if (useConverted && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        public string ReadLog(Stream stream)
        {
            if (this.store != null)
            {
                this.store.Dispose();
            }
            var reader = new StreamReader(stream);
            using (reader)
            {
                this.store = new LogStore();
                GC.Collect();
                this.store.StartAddMessages();
                this.totalMessages = 0;
                try
                {
                    var message = LogMessage.Create();
                    while (!reader.EndOfStream && this.NotCancelled)
                    {
                        var line = reader.ReadLine();

                        if (line == null)
                        {
                            break;
                        }

                        if (this.messageHead.IsMatch(line))
                        {
                            this.AddMessageToCache(message);
                            message = LogMessage.Create();
                        }

                        message.AddLine(line);
                    }
                    // Add last message
                    this.AddMessageToCache(message);
                }
                finally
                {
                    this.store.FinishAddMessages();
                }
            }
            return this.CreateRtf();
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

        public void TextFilter(string value)
        {
            this.textFilter = string.IsNullOrWhiteSpace(value)
                ? null
                : value;
        }

        private void Ordering(bool reverse)
        {
            this.reverseChronological = reverse;
        }

        public void ReadRecentFiles()
        {
            if (!File.Exists(this.recentFilesFilePath))
            {
                using (File.Open(this.recentFilesFilePath, FileMode.Create))
                {
                }
            }
            var files = File.ReadAllLines(this.recentFilesFilePath);
            this.recentFiles.Clear();
            this.view.ClearRecentFilesList();

            this.recentFiles.AddRange(files);
            this.recentFiles.Reverse();

            try
            {
                foreach (
                    var item in
                        from file in this.recentFiles
                        where !string.IsNullOrWhiteSpace(file) && File.Exists(file)
                        select file)
                {
                    this.view.CreateRecentFileItem(item);
                }
            }
            catch (Exception e)
            {
                Log.Instance.Error(e.Message, e);
            }
        }

        public void SaveRecentFiles()
        {
            this.recentFiles.Reverse();
            if (!this.recentFiles.Contains(this.view.LogPath))
            {
                this.recentFiles.Add(this.view.LogPath);
            }
            else
            {
                this.recentFiles.Remove(this.view.LogPath);
                this.recentFiles.Add(this.view.LogPath);
            }
            File.WriteAllLines(this.recentFilesFilePath, this.recentFiles);
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

        private bool CurrentPathCached
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.currentPath) &&
                       this.currentPath.Equals(this.view.LogPath, StringComparison.OrdinalIgnoreCase);
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

        private string CreateRtf()
        {
            this.byLevel.Clear();
            var rtfPath = Path.GetTempFileName();
            var doc = new RtfDocument(rtfPath);

            this.totalFiltered = this.store.CountMessages(this.minFilter, this.maxFilter, this.textFilter);

            if (this.CurrentPage > this.TotalPages || this.CurrentPage <= 0)
            {
                this.CurrentPage = 1;
            }

            var start = (this.CurrentPage - 1) * this.pageSize;
            this.store.ReadMessages(this.pageSize, m => this.AddMessage(doc, m), start, this.reverseChronological, this.minFilter, this.maxFilter,
                this.textFilter);
            doc.Close();
            return rtfPath;
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
            doc.AddText(NewLine);

            var txt = message.Body;
            if (string.IsNullOrWhiteSpace(txt))
            {
                doc.AddText(NewLine);
                return;
            }
            doc.AddText(txt.Trim(), message.BodyFormat);
            doc.AddText("\n\n\n");
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

        private string Convert()
        {
            if (!File.Exists(this.view.LogPath) || this.CurrentPathCached)
            {
                return this.view.LogPath;
            }
            return this.view.LogPath.ConvertToUtf8(() => this.NotCancelled);
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
                if (this.task != null)
                {
                    this.task.Dispose();
                }
                this.cancellation.Dispose();
                if (this.store != null)
                {
                    this.store.Dispose();
                }
            }
        }
    }
}