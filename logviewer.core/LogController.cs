using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Net.Sgoliver.NRtfTree.Core;
using Net.Sgoliver.NRtfTree.Util;
using logviewer.core.Properties;

namespace logviewer.core
{
    public class LogController
    {
        #region Constants and Fields

        private const string BigFileFormat = "{0:F2} {1} ({2} {3})";
        private const int BomLength = 3; // BOM (Byte Order Mark)
        private const int MeanLogStringLength = 70;
        private const string SmallFileFormat = "{0} {1}";

        private static readonly Dictionary<LogLevel, Color> levelsMap = new Dictionary<LogLevel, Color>
            {
                { LogLevel.Trace, Color.FromArgb(200, 200, 200) },
                { LogLevel.Debug, Color.FromArgb(100, 100, 100) },
                { LogLevel.Info, Color.Green },
                { LogLevel.Warn, Color.Orange },
                { LogLevel.Error, Color.Red },
                { LogLevel.Fatal, Color.DarkViolet }
            };

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

        private readonly List<string> recentFiles = new List<string>();
        private readonly string recentFilesFilePath;
        private readonly Regex regex;
        private readonly ILogView view;
        private bool cancelReading;
        private string debugMarker = "DEBUG";
        private string errorMarker = "ERROR";
        private string fatalMarker = "FATAL";
        private string infoMarker = "INFO";
        private string maxFilter;
        private string minFilter;
        private bool reverseChronological;
        private string textFilter;
        private string traceMarker = "TRACE";
        private string warnMarker = "WARN";

        #endregion

        #region Constructors and Destructors

        public LogController(ILogView view, string startMessagePattern, string recentFilesFilePath)
        {
            this.recentFilesFilePath = recentFilesFilePath;
            this.regex = new Regex(startMessagePattern, RegexOptions.Compiled);
            this.view = view;
            this.Messages = new List<LogMessage>();
        }

        #endregion

        #region Public Properties

        public string HumanReadableLogSize { get; private set; }
        public long LogSize { get; private set; }

        public List<LogMessage> Messages { get; private set; }

        #endregion

        #region Public Methods and Operators

        public void DefineTraceMarker(string marker)
        {
            this.traceMarker = marker;
        }

        public void DefineDebugMarker(string marker)
        {
            this.debugMarker = marker;
        }

        public void DefineInfoMarker(string marker)
        {
            this.infoMarker = marker;
        }

        public void DefineWarnMarker(string marker)
        {
            this.warnMarker = marker;
        }

        public void DefineErrorMarker(string marker)
        {
            this.errorMarker = marker;
        }

        public void DefineFatalMarker(string marker)
        {
            this.fatalMarker = marker;
        }

        public string ReadLog(string path)
        {
            this.cancelReading = false;
            Executive.SafeRun(this.Convert);
            return Executive.SafeRun<string, string>(this.ReadLogInternal, path);
        }

        public string ReadLog(Stream stream)
        {
            this.cancelReading = false;
            return Executive.SafeRun<string, Stream>(this.ReadLogInternal, stream);
        }

        public void CancelReading()
        {
            this.cancelReading = true;
        }

        public void MinFilter(string value)
        {
            this.minFilter = value;
        }

        public void MaxFilter(string value)
        {
            this.maxFilter = value;
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
            this.view.ClearRecentFilesList();
            foreach (var item in from file in files where !string.IsNullOrWhiteSpace(file) && File.Exists(file) select file)
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

        #endregion

        #region Methods

        private string ReadLogInternal(string path)
        {
            if (!File.Exists(path))
            {
                return string.Empty;
            }
            var reader = File.OpenText(path);
            return this.ReadLogInternal(reader);
        }

        private string ReadLogInternal(Stream stream)
        {
            return this.ReadLogInternal(new StreamReader(stream));
        }

        private string ReadLogInternal(StreamReader reader)
        {
            using (reader)
            {
                if (reader.BaseStream.CanSeek)
                {
                    this.LogSize = reader.BaseStream.Length;
                }
                this.CreateHumanReadableSize();

                return this.ReadLogTask(reader);
            }
        }

        private string ReadLogTask(StreamReader sr)
        {
            if (this.cancelReading)
            {
                return string.Empty;
            }
            this.Messages = new List<LogMessage>((int) this.LogSize / MeanLogStringLength);
            var message = new LogMessage { Strings = new List<string>() };

            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();

                if (line == null)
                {
                    break;
                }

                if (this.regex.IsMatch(line) && message.Strings.Count > 0)
                {
                    if (!this.Filter(message))
                    {
                        this.Messages.Add(message);
                    }
                    message.Strings = new List<string>();
                }

                message.Strings.Add(line);
            }
            if (message.Strings.Count > 0 && !this.Filter(message))
            {
                this.Messages.Add(message);
            }
            if (this.reverseChronological)
            {
                this.Messages.Reverse();
            }
            var rtfPath = Path.GetTempFileName();
            try
            {
                var doc = new RtfDocument(rtfPath);

                foreach (var msg in this.Messages)
                {
                    var formatBody = new RtfCharFormat
                        {
                            Color = this.Colorize(msg.Header),
                            Font = "Courier New",
                            Size = 10,
                            Bold = true
                        };
                    doc.AddText(msg.Header.Trim(), formatBody);
                    doc.AddText(Environment.NewLine);

                    var txt = msg.Body;
                    if (string.IsNullOrWhiteSpace(txt))
                    {
                        continue;
                    }
                    var format = new RtfCharFormat
                        {
                            Color = formatBody.Color,
                            Font = formatBody.Font,
                            Size = formatBody.Size
                        };

                    doc.AddText(txt.Trim(), format);
                    doc.AddText(Environment.NewLine);
                }
                doc.Close();

                var tree = new RtfTree();
                tree.LoadRtfFile(rtfPath);
                return tree.Rtf;
            }
            finally
            {
                if (File.Exists(rtfPath))
                {
                    File.Delete(rtfPath);
                }
            }
        }

        private bool Filter(LogMessage message)
        {
            var messageLevel = this.DetectLevel(message.Header);
            var filteredByLevel = messageLevel < this.DetectLevel(this.minFilter) || messageLevel > this.DetectLevel(this.maxFilter, LogLevel.Fatal);
            if (string.IsNullOrWhiteSpace(this.textFilter) || filteredByLevel)
            {
                return filteredByLevel;
            }
            var r = new Regex(this.textFilter, RegexOptions.IgnoreCase);
            return !r.IsMatch(message.ToString());
        }

        private Color Colorize(string line)
        {
            return levelsMap[this.DetectLevel(line)];
        }

        private LogLevel DetectLevel(string line, LogLevel defaultLevel = LogLevel.Trace)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return defaultLevel;
            }
            if (line.Contains(this.infoMarker))
            {
                return LogLevel.Info;
            }
            if (line.Contains(this.errorMarker))
            {
                return LogLevel.Error;
            }
            if (line.Contains(this.warnMarker))
            {
                return LogLevel.Warn;
            }
            if (line.Contains(this.fatalMarker))
            {
                return LogLevel.Fatal;
            }
            if (line.Contains(this.debugMarker))
            {
                return LogLevel.Debug;
            }
            if (line.Contains(this.traceMarker))
            {
                return LogLevel.Trace;
            }
            return defaultLevel;
        }

        private void Convert()
        {
            if (!File.Exists(this.view.LogPath))
            {
                return;
            }

            var stream = File.Open(this.view.LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var tmp = Path.GetTempFileName();
            try
            {
                using (stream)
                {
                    var b = new byte[BomLength];
                    stream.Read(b, 0, BomLength);
                    if (b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) // Do not convert file that is already in UTF-8
                    {
                        return;
                    }

                    stream.Seek(0, SeekOrigin.Begin);

                    var f = File.Create(tmp);
                    var newLineBytes = Encoding.UTF8.GetBytes(Environment.NewLine);
                    using (f)
                    {
                        f.WriteByte(0xEF);
                        f.WriteByte(0xBB);
                        f.WriteByte(0xBF);

                        var srcEncoding = Encoding.GetEncoding("windows-1251");
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
                                var asciiB = srcEncoding.GetBytes(line);
                                var utf8B = Encoding.Convert(srcEncoding, Encoding.UTF8, asciiB);
                                var utf8 = Encoding.UTF8.GetString(utf8B);
                                utf8B = Encoding.UTF8.GetBytes(utf8);
                                f.Write(utf8B, 0, utf8B.Length);
                                f.Write(newLineBytes, 0, newLineBytes.Length);
                            }
                        }
                    }
                }
                File.Delete(this.view.LogPath);
                File.Copy(tmp, this.view.LogPath);
            }
            finally
            {
                if (File.Exists(tmp))
                {
                    File.Delete(tmp);
                }
            }
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