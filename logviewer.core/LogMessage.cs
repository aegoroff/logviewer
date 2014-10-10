// Created by: egr
// Created at: 19.09.2012
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace logviewer.core
{
    public struct LogMessage
    {
        private const char NewLine = '\n';
        private const string All = "*";

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

        private static readonly IDictionary<string, Func<string, ParseResult<LogLevel>>> logLevelParsers = new Dictionary
            <string, Func<string, ParseResult<LogLevel>>>
        {
            {
                "LogLevel", 
                delegate(string s)
                {
                    LogLevel r;
                    var success = Enum.TryParse(s, true, out r);
                    return new ParseResult<LogLevel> { Result = success, Value = r };
                }
            },
            { "LogLevel.Trace", s => new ParseResult<LogLevel> { Result = true, Value = LogLevel.Trace } },
            { "LogLevel.Debug", s => new ParseResult<LogLevel> { Result = true, Value = LogLevel.Debug } },
            { "LogLevel.Info", s => new ParseResult<LogLevel> { Result = true, Value = LogLevel.Info } },
            { "LogLevel.Warn", s => new ParseResult<LogLevel> { Result = true, Value = LogLevel.Warn } },
            { "LogLevel.Error", s => new ParseResult<LogLevel> { Result = true, Value = LogLevel.Error } },
            { "LogLevel.Fatal", s => new ParseResult<LogLevel> { Result = true, Value = LogLevel.Fatal } }
        };

        private void DetectLevel()
        {
            if (semantic == null)
            {
                return;
            }
            foreach (var item in semantic)
            {
                var matchedData = item.Value;
                foreach (var rule in item.Key.CastingRules.Where(rule => rule.Key != All))
                {
                    if (!matchedData.Contains(rule.Key))
                    {
                        continue;
                    }
                    if (this.ApplyRule(item, rule.Key, matchedData))
                    {
                        return;
                    }
                }
                this.ApplyRule(item, All, matchedData);
            }
        }

        private bool ApplyRule(KeyValuePair<Semantic, string> item, string rule, string matchedData)
        {
            if (!item.Key.CastingRules.ContainsKey(rule))
            {
                return false;
            }
            var castingType = item.Key.CastingRules[rule];
            if (!castingType.Contains("LogLevel") || !logLevelParsers.ContainsKey(castingType))
            {
                return false;
            }
            var result = logLevelParsers[castingType](matchedData);
            if (result.Result)
            {
                this.Level = result.Value;
            }
            return result.Result;
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
            this.DetectLevel();
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