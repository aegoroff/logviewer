using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private const int MeanLogStringLength = 70;
        private const string SmallFileFormat = "{0} {1}";
        private const string NewLine = "\n";
        private const int DefaultPageSize = 10000;

        private static readonly string[] sizes = new[]
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
        private readonly int pageSize;

        private readonly List<string> recentFiles = new List<string>();
        private readonly string recentFilesFilePath;
        private readonly Regex regex;
        private readonly ILogView view;

        private bool cancelReading;
        private string currentPath;

        private LogLevel maxFilter = LogLevel.Fatal;
        private List<LogMessage> messagesCache;
        private LogLevel minFilter = LogLevel.Trace;
        private bool reverseChronological;
        private string textFilter;

        #endregion

        #region Constructors and Destructors

        public MainController(ILogView view, string startMessagePattern, string recentFilesFilePath, IEnumerable<string> levels, int pageSize = DefaultPageSize)
        {
            CurrentPage = 1;
            this.recentFilesFilePath = recentFilesFilePath;
            this.pageSize = pageSize <= 0 ? DefaultPageSize : pageSize;
            this.markers = new List<Regex>();
            this.markers.AddRange(levels.Select(level => level.ToMarker()));
            this.regex = new Regex(startMessagePattern, RegexOptions.Compiled);
            this.view = view;
            this.Messages = new List<LogMessage>();
        }

        #endregion

        #region Public Properties

        public string HumanReadableLogSize { get; private set; }

        public long LogSize { get; private set; }

        public List<LogMessage> Messages { get; private set; }

        public int TotalMessages
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
                    return 0;
                }
                return (int) Math.Ceiling(this.DisplayedMessages / (float) this.pageSize);
            }
        }

        public int DisplayedMessages
        {
            get { return this.Messages.Count; }
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
            }
            finally
            {
                File.Delete(path);
            }
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
            if (!this.view.IsBusy || this.view.CancellationPending)
            {
                return;
            }
            this.cancelReading = true;
            this.view.CancelRead();
        }

        public void ClearCache()
        {
            this.currentPath = null;
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
            this.textFilter = value;
        }

        public void Ordering(bool reverse)
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
            this.recentFiles.AddRange(files);
            this.recentFiles.Reverse();
            this.view.ClearRecentFilesList();
            foreach (
                var item in from file in files where !string.IsNullOrWhiteSpace(file) && File.Exists(file) select file)
            {
                this.view.CreateRecentFileItem(item);
            }
        }

        public void SaveRecentFiles()
        {
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
            this.view.LogPath = this.view.LogFileName;
            if (this.view.IsBusy)
            {
                return;
            }
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
                this.messagesCache = new List<LogMessage>((int) this.LogSize / MeanLogStringLength);
                var message = new LogMessage { Strings = new List<string>() };

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    if (this.regex.IsMatch(line))
                    {
                        this.AddMessageToCache(message);
                        message.Strings = new List<string>();
                    }

                    message.Strings.Add(line);
                }
                this.AddMessageToCache(message);
            }
        }

        private void AddMessageToCache(LogMessage message)
        {
            if (message.Strings.Count == 0)
            {
                return;
            }
            message.Level = this.DetectLevel(message.Header);
            this.messagesCache.Add(message);
        }

        private string CreateRtf()
        {
            this.Messages = new List<LogMessage>(this.messagesCache.Where(m => !this.Filter(m)));
            this.byLevel.Clear();
            if (this.reverseChronological)
            {
                this.Messages.Reverse();
            }
            var rtfPath = Path.GetTempFileName();
            var doc = new RtfDocument(rtfPath);

            foreach (var m in this.Messages)
            {
                this.AddMessage(doc, m);
            }
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

        private bool Filter(LogMessage message)
        {
            var filteredByLevel = message.Level < this.minFilter || message.Level > this.maxFilter;
            if (string.IsNullOrWhiteSpace(this.textFilter) || filteredByLevel)
            {
                return filteredByLevel;
            }
            var r = new Regex(this.textFilter, RegexOptions.IgnoreCase);
            return !r.IsMatch(message.ToString());
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