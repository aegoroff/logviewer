// Created by: egr
// Created at: 19.09.2012
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Net.Sgoliver.NRtfTree.Util;

namespace logviewer.core
{
    public struct LogMessage
    {
        private const char NewLine = '\n';

        private static readonly Dictionary<LogLevel, RtfCharFormat> bodyFormatsMap = new Dictionary
            <LogLevel, RtfCharFormat>
        {
            { LogLevel.None, FormatBody(LogLevel.None) },
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
            { LogLevel.None, FormatHead(LogLevel.None) },
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
            this.semantic = null;
            this.bodyBuilder = null;
        }

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.head) && string.IsNullOrWhiteSpace(this.body) &&
                       this.bodyBuilder.Length == 0;
            }
        }

        public long Ix
        {
            get { return this.ix; }
            set { this.ix = value; }
        }

        public string Header
        {
            get { return this.head ?? string.Empty; }
        }

        public string Body
        {
            get
            {
                return this.body ??
                       (this.bodyBuilder.Length == 1 && this.bodyBuilder[0] == NewLine
                           ? string.Empty
                           : this.bodyBuilder.ToString());
            }
        }

        public RtfCharFormat HeadFormat
        {
            get { return headerFormatsMap[this.Level]; }
        }

        public RtfCharFormat BodyFormat
        {
            get { return bodyFormatsMap[this.Level]; }
        }

        public bool HasSemantic
        {
            get { return this.semantic != null; }
        }

        public void AddLine(string line)
        {
            if (this.head == null)
            {
                this.head = line;
            }
            else
            {
                this.bodyBuilder.Append(line);
                this.bodyBuilder.Append(NewLine);
            }
        }

        public void AddSemantic(IDictionary<Semantic, string> sem)
        {
            this.semantic = sem;
        }

        public void ApplySemantic(Func<IDictionary<Semantic, string>, LogLevel> method)
        {
            this.Level = method(this.semantic);
        }

        public void Cache()
        {
            if (this.head != null && this.body != null)
            {
                return;
            }
            if (this.bodyBuilder.Length > 0)
            {
                this.bodyBuilder.Remove(this.bodyBuilder.Length - 1, 1);
            }
            this.body = this.bodyBuilder.ToString();
            this.bodyBuilder.Clear();
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
            RtfCharFormat f = FormatHead(level);
            f.Bold = false;
            return f;
        }

        internal static Color Colorize(LogLevel level)
        {
            if (levelsMap == null)
            {
                levelsMap = new Dictionary<LogLevel, Color>
                {
                    { LogLevel.None, Color.Black },
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

        internal static void UpdateColor(LogLevel level, Color color)
        {
            if (levelsMap == null || !levelsMap.ContainsKey(level))
            {
                return;
            }
            levelsMap[level] = color;
        }

        public static LogMessage Create()
        {
            return new LogMessage { Level = LogLevel.None, bodyBuilder = new StringBuilder() };
        }

        #region Constants and Fields

        internal LogLevel Level;
        private string body;
        private StringBuilder bodyBuilder;
        private string head;
        private long ix;
        private IDictionary<Semantic, string> semantic;

        #endregion
    }
}