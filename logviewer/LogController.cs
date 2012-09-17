using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Net.Sgoliver.NRtfTree.Core;
using Net.Sgoliver.NRtfTree.Util;

namespace logviewer
{
    public class LogController
    {
        #region Constants and Fields

        private const string BigFileFormat = "{0:F2} {1} ({2} {3})";
        private const int BomLength = 3; // BOM (Byte Order Mark)
        private const int MeanLogStringLength = 70;
        private const string SmallFileFormat = "{0} {1}";
        private const string StartMessagePattern = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}.*";
        private static readonly Regex regex = new Regex(StartMessagePattern, RegexOptions.Compiled);

        private static Dictionary<Color, LogLevel> levelsMap = new Dictionary<Color, LogLevel>
            {
                { Color.FromArgb(200, 200, 200), LogLevel.Trace },
                { Color.FromArgb(100, 100, 100), LogLevel.Debug },
                { Color.Green, LogLevel.Info },
                { Color.Orange, LogLevel.Warn },
                { Color.Red, LogLevel.Error },
                { Color.DarkViolet, LogLevel.Fatal }
            };

        private static readonly string[] sizes = new[]
            {
                "Bytes",
                "Kb",
                "Mb",
                "Gb",
                "Tb",
                "Pb",
                "Eb",
            };

        private readonly ILogView view;
        private bool cancelReading;
        private string filter;

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

        public void Filter(string value)
        {
            this.filter = value;
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

                if (regex.IsMatch(line) && message.Strings.Count > 0)
                {
                    this.Messages.Add(message);
                    message.Strings = new List<string>();
                }

                message.Strings.Add(line);
            }
            this.Messages.Add(message);
            this.Messages.Reverse();
            const string rtfPath = "log.rtf";
            try
            {
                var doc = new RtfDocument(rtfPath);

                foreach (var msg in this.Messages)
                {
                    var color = Colorize(msg.Strings[0]);
                    var messageLevel = LogLevel.Trace;
                    if (levelsMap.ContainsKey(color))
                    {
                        messageLevel = levelsMap[color];
                    }
                    if (!string.IsNullOrWhiteSpace(this.filter))
                    {
                        var filterColor = Colorize(this.filter);
                        var filterLevel = LogLevel.Trace;
                        if(levelsMap.ContainsKey(filterColor))
                        {
                            filterLevel = levelsMap[filterColor];
                        }
                        if (messageLevel < filterLevel)
                        {
                            continue;
                        }
                    }
                    var format = new RtfCharFormat
                        {
                            Color = color,
                            Font = "Courier New",
                            Size = 10
                        };
                    var txt = msg.ToString();
                    doc.AddText(txt, format);
                    if (!txt.EndsWith("\n"))
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
            var color = Color.Black;
            if (line.Contains("ERROR"))
            {
                color = Color.Red;
            }
            else if (line.Contains("WARN"))
            {
                color = Color.Orange;
            }
            else if (line.Contains("INFO"))
            {
                color = Color.Green;
            }
            else if (line.Contains("FATAL"))
            {
                color = Color.DarkViolet;
            }
            else if (line.Contains("DEBUG"))
            {
                color = Color.FromArgb(100, 100, 100);
            }
            else if (line.Contains("TRACE"))
            {
                color = Color.FromArgb(200, 200, 200);
            }
            return color;
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

    public struct LogMessage
    {
        #region Constants and Fields

        internal IList<string> Strings;

        #endregion

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this.Strings);
        }
    }
}