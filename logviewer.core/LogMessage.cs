using System.Collections.Generic;
using System.Drawing;
using System.Text;
using logviewer.rtf.Rtf.Contents.Text;

namespace logviewer.core
{
    public struct LogMessage
    {
        private const string NewLine = "\n";

        private static Dictionary<LogLevel, Color> levelsMap;

        public LogMessage(string header, string body, LogLevel level)
        {
            this.head = header;
            this.body = body;
            this.Level = level;
            this.strings = null;
        }

        public bool IsEmpty
        {
            get { return this.head == null && this.body == null && (this.strings == null || this.strings.Count == 0); }
        }
        
        public string Header
        {
            get { return this.head ?? (this.IsEmpty ? string.Empty : this.strings[0]); }
        }

        public string Body
        {
            get { return this.body ?? (this.strings.Count < 2 ? string.Empty : this.ToString(1)); }
        }

        public RtfFormattedText HeadFormat
        {
            get
            {
                return new RtfFormattedText(Header.Trim(), RtfCharacterFormatting.Bold)
                {
                    TextColorIndex = (int)Level + 1
                };
            }
        }

        public RtfFormattedText BodyFormat
        {
            get
            {
                var txt = Body ?? "\n";
                return new RtfFormattedText(txt.Trim(), RtfCharacterFormatting.Regular)
                {
                    TextColorIndex = (int)Level + 1
                };
            }
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

        internal static Color Colorize(LogLevel level)
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

        public override string ToString()
        {
            return this.head == null && this.body == null ? this.ToString(0) : head + (string.IsNullOrEmpty(body) ? string.Empty : NewLine + body);
        }

        private string ToString(int start)
        {
            var sb = new StringBuilder();
            var count = this.strings.Count;
// ReSharper disable ForCanBeConvertedToForeach
            for (var i = start; i < count; i++)
// ReSharper restore ForCanBeConvertedToForeach
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

        #endregion
    }
}