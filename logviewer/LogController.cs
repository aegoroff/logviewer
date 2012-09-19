using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Net.Sgoliver.NRtfTree.Core;
using Net.Sgoliver.NRtfTree.Util;
using logviewer.Properties;

namespace logviewer
{
    public class LogController
    {
        #region Constants and Fields

        private const string BigFileFormat = "{0:F2} {1} ({2} {3})";
        private const int BomLength = 3; // BOM (Byte Order Mark)
        private const int MeanLogStringLength = 70;
        private const string SmallFileFormat = "{0} {1}";
        private static readonly Regex regex = new Regex(ConfigurationManager.AppSettings["StartMessagePattern"], RegexOptions.Compiled);

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

        private readonly ILogView view;
        private bool cancelReading;
        private string maxFilter;
        private string minFilter;
        private bool reverseChronological;

        #endregion

        #region Constructors and Destructors

        public LogController(ILogView view)
        {
            this.view = view;
            this.Messages = new List<LogMessage>();
            Executive.SafeRun(this.Convert);
        }

        #endregion

        #region Public Properties

        public string HumanReadableLogSize { get; private set; }
        public long LogSize { get; private set; }

        public List<LogMessage> Messages { get; private set; }

        #endregion

        #region Public Methods and Operators

        public string ReadLog(string path)
        {
            this.cancelReading = false;
            return Executive.SafeRun<string, string>(this.ReadLogInternal, path);
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

        public void Ordering(bool reverse)
        {
            this.reverseChronological = reverse;
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
            using (reader)
            {
                this.LogSize = reader.BaseStream.Length;
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

                if (regex.IsMatch(line) && message.Strings.Count > 0)
                {
                    this.Messages.Add(message);
                    message.Strings = new List<string>();
                }

                message.Strings.Add(line);
            }
            this.Messages.Add(message);
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
                    var messageLevel = DetectLevel(msg.Header);

                    if (messageLevel < DetectLevel(this.minFilter) || messageLevel > DetectLevel(this.maxFilter, LogLevel.Fatal))
                    {
                        continue;
                    }

                    var formatBody = new RtfCharFormat
                        {
                            Color = Colorize(msg.Header),
                            Font = "Courier New",
                            Size = 10,
                            Bold = true
                        };
                    doc.AddText(msg.Header.Trim(), formatBody);
                    try
                    {
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
                    }
                    finally
                    {
                        doc.AddText(Environment.NewLine);
                    }
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

        private static Color Colorize(string line)
        {
            return levelsMap[DetectLevel(line)];
        }

        private static LogLevel DetectLevel(string line, LogLevel defaultLevel = LogLevel.Trace)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return defaultLevel;
            }
            if (line.Contains("ERROR"))
            {
                return LogLevel.Error;
            }
            if (line.Contains("WARN"))
            {
                return LogLevel.Warn;
            }
            if (line.Contains("INFO"))
            {
                return LogLevel.Info;
            }
            if (line.Contains("FATAL"))
            {
                return LogLevel.Fatal;
            }
            if (line.Contains("DEBUG"))
            {
                return LogLevel.Debug;
            }
            if (line.Contains("TRACE"))
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
            byte[] b;
            using (stream)
            {
                b = new byte[BomLength];
                stream.Read(b, 0, BomLength);
            }
            if (b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) // Do not convert file that is already in UTF-8
            {
                return;
            }
            var srcEncoding = Encoding.GetEncoding("windows-1251");
            var log = File.ReadAllText(this.view.LogPath, srcEncoding);
            var asciiBytes = srcEncoding.GetBytes(log);
            var utf8Bytes = Encoding.Convert(srcEncoding, Encoding.UTF8, asciiBytes);
            var utf8 = Encoding.UTF8.GetString(utf8Bytes);
            File.WriteAllText(this.view.LogPath, utf8, Encoding.UTF8);
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