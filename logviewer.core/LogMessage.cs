// Created by: egr
// Created at: 19.09.2012
// © 2012-2013 Alexander Egorov

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Net.Sgoliver.NRtfTree.Util;

namespace logviewer.core
{
    public struct LogMessage
    {
        private const string NewLine = "\n";

        private static readonly Dictionary<LogLevel, RtfCharFormat> bodyFormatsMap = new Dictionary
            <LogLevel, RtfCharFormat>
        {
            { LogLevel.Trace, FormatBody(LogLevel.Trace) },
            { LogLevel.Debug, FormatBody(LogLevel.Debug) },
            { LogLevel.Info, FormatBody(LogLevel.Info) },
            { LogLevel.Warn, FormatBody(LogLevel.Warn) },
            { LogLevel.Error, FormatBody(LogLevel.Error) },
            { LogLevel.Fatal, FormatBody(LogLevel.Fatal) },
        };

        private static readonly Dictionary<LogLevel, RtfCharFormat> headerFormatsMap = new Dictionary
            <LogLevel, RtfCharFormat>
        {
            { LogLevel.Trace, FormatHead(LogLevel.Trace) },
            { LogLevel.Debug, FormatHead(LogLevel.Debug) },
            { LogLevel.Info, FormatHead(LogLevel.Info) },
            { LogLevel.Warn, FormatHead(LogLevel.Warn) },
            { LogLevel.Error, FormatHead(LogLevel.Error) },
            { LogLevel.Fatal, FormatHead(LogLevel.Fatal) },
        };

        private static Dictionary<LogLevel, Color> levelsMap;

        public LogMessage(string header, string body, LogLevel level)
        {
            this.head = header;
            this.body = body;
            this.Level = level;
            this.ix = 0;
            this.strings = null;
        }

        public bool IsEmpty
        {
            get { return string.IsNullOrWhiteSpace(this.head) && string.IsNullOrWhiteSpace(this.body) && (this.strings == null || this.strings.Count == 0 || this.strings.All(string.IsNullOrWhiteSpace)); }
        }

        public long Ix
        {
            get { return this.ix; }
            set { this.ix = value; }
        }

        public string Header
        {
            get { return this.head ?? (this.IsEmpty ? string.Empty : this.strings[0]); }
        }

        public string Body
        {
            get { return this.body ?? (this.strings.Count < 2 ? string.Empty : this.ToString(1)); }
        }

        public RtfCharFormat HeadFormat
        {
            get { return headerFormatsMap[this.Level]; }
        }

        public RtfCharFormat BodyFormat
        {
            get { return bodyFormatsMap[this.Level]; }
        }

        public void AddLine(string line)
        {
            this.strings.Add(line);
        }

        public void Cache()
        {
            if (this.head != null && this.body != null)
            {
                return;
            }
            this.head = this.Header;
            this.body = this.Body;
            this.strings.Clear();
            this.strings = null;
        }

        private static RtfCharFormat FormatHead(LogLevel level)
        {
            return new RtfCharFormat
            {
                Color = Colorize(level),
                Font = "Courier New",
                Size = 10,
                Bold = true
            };
        }

        private static RtfCharFormat FormatBody(LogLevel level)
        {
            var f = FormatHead(level);
            f.Bold = false;
            return f;
        }

        private static Color Colorize(LogLevel level)
        {
            if (levelsMap == null)
            {
                levelsMap = new Dictionary<LogLevel, Color>
                {
                    { LogLevel.Trace, Color.FromArgb(200, 200, 200) },
                    { LogLevel.Debug, Color.FromArgb(100, 100, 100) },
                    { LogLevel.Info, Color.Green },
                    { LogLevel.Warn, Color.Orange },
                    { LogLevel.Error, Color.Red },
                    { LogLevel.Fatal, Color.DarkViolet }
                };
            }
            return levelsMap[level];
        }

        private string ToString(int start)
        {
            var sb = new StringBuilder();
            var count = this.strings.Count;
            for (var i = start; i < count; i++)
            {
                sb.Append(this.strings[i]);
                if (i < count - 1)
                {
                    sb.Append(NewLine);
                }
            }
            return sb.ToString();
        }

        public static LogMessage Create()
        {
            return new LogMessage { strings = new List<string>(), Level = LogLevel.Trace };
        }

        #region Constants and Fields

        internal LogLevel Level;
        private string body;
        private string head;
        private List<string> strings;
        private long ix;

        #endregion
    }
}