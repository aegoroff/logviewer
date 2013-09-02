using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets;
using Net.Sgoliver.NRtfTree.Util;
using Ude;
using logviewer.core.Properties;

namespace logviewer.core
{
    public class MainController
    {
        #region Constants and Fields

        private const string BigFileFormat = "{0:F2} {1} ({2} {3})";
        private const int MeanLogStringLength = 50;
        private const string SmallFileFormat = "{0} {1}";
        private const string NewLine = "\n";
        private const int DefaultPageSize = 10000;

        private static readonly string[] sizes =
        {
            Resources.SizeBytes,
            Resources.SizeKBytes,
            Resources.SizeMBytes,
            Resources.SizeGBytes,
            Resources.SizeTBytes,
            Resources.SizePBytes,
            Resources.SizeEBytes
        };

        private readonly Dictionary<LogLevel, int> byLevel = new Dictionary<LogLevel, int>();

        private readonly List<Regex> markers;
        private readonly Regex messageHead;
        private readonly int pageSize;

        private readonly List<string> recentFiles = new List<string>();
        private readonly string recentFilesFilePath;

        private bool cancelReading;
        private string currentPath;

        private LogLevel maxFilter = LogLevel.Fatal;
        private List<LogMessage> messagesCache;
        private LogLevel minFilter = LogLevel.Trace;
        private bool reverseChronological;
        private Regex textFilter;
        private ILogView view;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        #endregion

        #region Constructors and Destructors

        public MainController(string startMessagePattern, string recentFilesFilePath, IEnumerable<string> levels,
                              int pageSize = DefaultPageSize)
        {
            this.CurrentPage = 1;
            this.RebuildMessages = true;
            this.recentFilesFilePath = recentFilesFilePath;
            this.pageSize = pageSize <= 0 ? DefaultPageSize : pageSize;
            this.markers = new List<Regex>();
            this.markers.AddRange(levels.Select(level => level.ToMarker()));
            this.messageHead = new Regex(startMessagePattern, RegexOptions.Compiled);
            this.messages = new List<LogMessage>();
        }

        #endregion

        #region Public Properties

        private List<LogMessage> messages;
        public string HumanReadableLogSize { get; private set; }

        public long LogSize { get; private set; }

        public bool RebuildMessages { get; set; }

        public int MessagesCount
        {
            get { return this.messages.Count; }
        }

        private int TotalMessages
        {
            get { return this.messagesCache == null ? 0 : this.messagesCache.Count; }
        }

        public int CurrentPage { get; set; }

        public int TotalPages
        {
            get
            {
                if (this.messagesCache == null)
                {
                    return 1;
                }
                return (int) Math.Ceiling(this.messages.Count / (float) this.pageSize);
            }
        }

        public int DisplayedMessages
        {
            get
            {
                var start = (this.CurrentPage - 1) * this.pageSize;
                var tail = this.messages.Count - start;
                var finish = Math.Min(tail, this.pageSize);
                return Math.Min(finish, this.messages.Count);
            }
        }

        #endregion

        #region Public Methods and Operators

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

        public void ReadLog(int min, int max, string filter, bool reverse)
        {
            this.MinFilter(min);
            this.MaxFilter(max);
            this.TextFilter(filter);
            this.Ordering(reverse);
            var path = string.Empty;
            Task.Factory.StartNew(() => { path = Executive.SafeRun<string>(this.ReadLog); }, this.cancellation.Token).ContinueWith(delegate
            {
                this.view.HumanReadableLogSize = this.HumanReadableLogSize;
                this.view.LogInfo = string.Format(this.view.OriginalLogInfo, this.DisplayedMessages,
                    this.TotalMessages, this.CountMessages(LogLevel.Trace), this.CountMessages(LogLevel.Debug),
                    this.CountMessages(LogLevel.Info), this.CountMessages(LogLevel.Warn), this.CountMessages(LogLevel.Error),
                    this.CountMessages(LogLevel.Fatal), this.MessagesCount);
                this.LoadLog(path);
                this.ReadRecentFiles();
                this.view.FocusOnTextFilterControl();
            }, this.cancellation.Token);
        }

        public string ReadLog()
        {
            if (this.minFilter > this.maxFilter && this.maxFilter >= LogLevel.Trace)
            {
                throw new ArgumentException(Resources.MinLevelGreaterThenMax);
            }
            this.cancelReading = false;
            var convertedPath = Executive.SafeRun<string>(this.Convert);
            var useConverted = !string.IsNullOrWhiteSpace(convertedPath) &&
                               !convertedPath.Equals(this.view.LogPath, StringComparison.OrdinalIgnoreCase);
            var filePath = useConverted ? convertedPath : this.view.LogPath;
            try
            {
                return Executive.SafeRun<string, string>(this.ReadLogInternal, filePath);
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
            this.cancelReading = false;
            return Executive.SafeRun<string, Stream>(this.ReadLogInternal, stream);
        }

        public void CancelReading()
        {
            if (this.cancellation.IsCancellationRequested)
            {
                return;
            }
            this.cancelReading = true;
            this.cancellation.Cancel();
        }

        public void ClearCache()
        {
            this.currentPath = null;
            this.RebuildMessages = true;
        }

        public void MinFilter(int value)
        {
            this.minFilter = (LogLevel) value;
        }

        public void MaxFilter(int value)
        {
            this.maxFilter = (LogLevel) value;
        }

        public void TextFilter(string value)
        {
            this.textFilter = string.IsNullOrWhiteSpace(value)
                                  ? null
                                  : new Regex(value,
                                              RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
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

        private bool CurrentPathCached
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.currentPath) &&
                       this.currentPath.Equals(this.view.LogPath, StringComparison.OrdinalIgnoreCase);
            }
        }

        private string ReadLogInternal(string path)
        {
            if (!File.Exists(this.view.LogPath))
            {
                return string.Empty;
            }
            if (!this.CurrentPathCached)
            {
                this.currentPath = this.view.LogPath;
                var reader = File.OpenText(path);
                this.CacheStream(reader);
            }
            return this.CreateRtf();
        }

        private string ReadLogInternal(Stream stream)
        {
            this.CacheStream(new StreamReader(stream));
            return this.CreateRtf();
        }

        private void CacheStream(StreamReader reader)
        {
            using (reader)
            {
                if (reader.BaseStream.CanSeek)
                {
                    this.LogSize = reader.BaseStream.Length;
                }
                this.CreateHumanReadableSize();
                if (this.cancelReading)
                {
                    return;
                }
                var logCharsCount = (int) this.LogSize / sizeof (char);
                this.messagesCache = new List<LogMessage>(logCharsCount / MeanLogStringLength);
                var message = LogMessage.Create();
                while (!reader.EndOfStream)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
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
        }

        private void AddMessageToCache(LogMessage message)
        {
            if (message.IsEmpty)
            {
                return;
            }
            message.Level = this.DetectLevel(message.Header);
            message.Cache();
            this.messagesCache.Add(message);
        }

        private string CreateRtf()
        {
            if (this.RebuildMessages)
            {
                this.messages = new List<LogMessage>(this.pageSize);
                this.ReadFromCache();
            }
            this.byLevel.Clear();
            var rtfPath = Path.GetTempFileName();
            var doc = new RtfDocument(rtfPath);

            if (this.CurrentPage > this.TotalPages || this.CurrentPage <= 0)
            {
                this.CurrentPage = 1;
            }

            var start = (this.CurrentPage - 1) * this.pageSize;
            var finish = Math.Min(start + this.pageSize, this.messages.Count);
            for (var i = start; i < finish; i++)
            {
                if (cancellation.IsCancellationRequested)
                {
                    return string.Empty;
                }
                this.AddMessage(doc, this.messages[i]);
            }
            doc.Close();
            return rtfPath;
        }

        private void ReadFromCache()
        {
            if (this.reverseChronological)
            {
                for (var i = this.messagesCache.Count - 1; i >= 0; i--)
                {
                    if (!this.Filter(this.messagesCache[i]))
                    {
                        this.messages.Add(this.messagesCache[i]);
                    }
                }
            }
            else
            {
                // IMPORTANT: dont use LINQ due to performance reason
// ReSharper disable ForCanBeConvertedToForeach
                for (var i = 0; i < this.messagesCache.Count; i++)
// ReSharper restore ForCanBeConvertedToForeach
                {
                    if (!this.Filter(this.messagesCache[i]))
                    {
                        this.messages.Add(this.messagesCache[i]);
                    }
                }
            }
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

        private bool Filter(LogMessage message)
        {
            var filteredByLevel = message.Level < this.minFilter || message.Level > this.maxFilter;
            if (this.textFilter == null || filteredByLevel)
            {
                return filteredByLevel;
            }
            return !this.textFilter.IsMatch(message.ToString());
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
                    return (LogLevel) i;
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

            var stream = File.Open(this.view.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var result = Path.GetTempFileName();
            using (stream)
            {
                Encoding srcEncoding;
                var detector = new CharsetDetector();
                detector.Feed(stream);
                detector.DataEnd();
                if (detector.Charset != null)
                {
                    srcEncoding = Encoding.GetEncoding(detector.Charset);
                }
                else
                {
                    return this.view.LogPath;
                }

                if (srcEncoding.Equals(Encoding.UTF8) || srcEncoding.Equals(Encoding.ASCII))
                {
                    return this.view.LogPath;
                }

                stream.Seek(0, SeekOrigin.Begin);

                var f = File.Create(result);
                using (f)
                {
                    f.WriteByte(0xEF);
                    f.WriteByte(0xBB);
                    f.WriteByte(0xBF);

                    var sr = new StreamReader(stream, srcEncoding);
                    using (sr)
                    {
                        while (!sr.EndOfStream)
                        {
                            var line = sr.ReadLine();
                            if (line == null)
                            {
                                break;
                            }
                            f.WriteLine(line, srcEncoding, Encoding.UTF8);
                        }
                    }
                }
            }
            return result;
        }

        private void CreateHumanReadableSize()
        {
            var normalized = new FileSize((ulong) this.LogSize);
            if (normalized.Unit == SizeUnit.Bytes)
            {
                this.HumanReadableLogSize = string.Format(CultureInfo.CurrentCulture, SmallFileFormat, normalized.Bytes,
                                                          sizes[(int) normalized.Unit]);
            }
            else
            {
                this.HumanReadableLogSize = string.Format(CultureInfo.CurrentCulture, BigFileFormat, normalized.Value,
                                                          sizes[(int) normalized.Unit], normalized.Bytes,
                                                          sizes[(int) SizeUnit.Bytes]);
            }
        }

        #endregion
    }
}