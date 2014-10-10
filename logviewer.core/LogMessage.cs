// Created by: egr
// Created at: 19.09.2012
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Text;

namespace logviewer.core
{
    public struct LogMessage
    {
        private const char NewLine = '\n';

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

        private static readonly Semantic levelSemantic = new Semantic("level");

        private static LogLevel DetectLevel(IDictionary<Semantic, string> match)
        {
            if (match == null)
            {
                return LogLevel.None;
            }
            string level;
            if (!match.TryGetValue(levelSemantic, out level))
            {
                return LogLevel.None;
            }
            LogLevel result;
            return Enum.TryParse(level, true, out result) ? result : LogLevel.None;
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
            this.Level = DetectLevel(this.semantic);
            this.body = this.bodyBuilder.ToString();
            this.bodyBuilder.Clear();
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