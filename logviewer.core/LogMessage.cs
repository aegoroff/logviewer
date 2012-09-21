using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Net.Sgoliver.NRtfTree.Util;

namespace logviewer.core
{
    public struct LogMessage
    {
        private static readonly Dictionary<LogLevel, RtfCharFormat> bodyFormatsMap = new Dictionary<LogLevel, RtfCharFormat>
            {
                { LogLevel.Trace, FormatBody(LogLevel.Trace) },
                { LogLevel.Debug, FormatBody(LogLevel.Debug) },
                { LogLevel.Info, FormatBody(LogLevel.Info) },
                { LogLevel.Warn, FormatBody(LogLevel.Warn) },
                { LogLevel.Error, FormatBody(LogLevel.Error) },
                { LogLevel.Fatal, FormatBody(LogLevel.Fatal) },
            };

        private static readonly Dictionary<LogLevel, RtfCharFormat> headerFormatsMap = new Dictionary<LogLevel, RtfCharFormat>
            {
                { LogLevel.Trace, FormatHead(LogLevel.Trace) },
                { LogLevel.Debug, FormatHead(LogLevel.Debug) },
                { LogLevel.Info, FormatHead(LogLevel.Info) },
                { LogLevel.Warn, FormatHead(LogLevel.Warn) },
                { LogLevel.Error, FormatHead(LogLevel.Error) },
                { LogLevel.Fatal, FormatHead(LogLevel.Fatal) },
            };

        private static Dictionary<LogLevel, Color> levelsMap;

        public string Header
        {
            get { return this.Strings.Count == 0 ? string.Empty : this.Strings[0]; }
        }

        public string Body
        {
            get { return this.Strings.Count < 2 ? string.Empty : string.Join(Environment.NewLine, this.MessageBody()); }
        }

        public RtfCharFormat HeadFormat
        {
            get { return headerFormatsMap[this.Level]; }
        }

        public RtfCharFormat BodyFormat
        {
            get { return bodyFormatsMap[this.Level]; }
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

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this.Strings);
        }

        private IEnumerable<string> MessageBody()
        {
            var i = 0;
            return this.Strings.Where(s => i++ > 0);
        }

        #region Constants and Fields

        internal LogLevel Level;
        internal IList<string> Strings;

        #endregion
    }
}