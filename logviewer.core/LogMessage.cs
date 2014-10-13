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

        private static readonly IDictionary<string, Func<string, ParseResult<DateTime>>> dateTimeParsers = new Dictionary
            <string, Func<string, ParseResult<DateTime>>>
        {
            {
                "DateTime", 
                delegate(string s)
                {
                    DateTime r;
                    var success = DateTime.TryParse(s, out r);
                    return new ParseResult<DateTime> { Result = success, Value = r };
                }
            },
        };

        private void DetectLevel()
        {
            if (this.semantic == null)
            {
                return;
            }
            var r = this.RunSemanticAction(new SemanticAction<LogLevel> { Key = "LogLevel", Parsers = logLevelParsers });
            if (r.Result)
            {
                this.SetLevel(r.Value);
            }
            this.RunSemanticAction(new SemanticAction<DateTime> { Key = "DateTime", Parsers = dateTimeParsers });
        }

        private ParseResult<T> RunSemanticAction<T>(SemanticAction<T> logLevelSemantic)
        {
// ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var item in this.semantic)
            {
                var r = this.ApplySemantic(item, logLevelSemantic);
                if (r.Result)
                {
                    return r;
                }
            }
            return new ParseResult<T>();
        }

        private ParseResult<T> ApplySemantic<T>(KeyValuePair<Semantic, string> item, SemanticAction<T> action)
        {
            var matchedData = item.Value;
            foreach (var rule in item.Key.CastingRules.Where(rule => rule.Key != All))
            {
                if (!matchedData.Contains(rule.Key))
                {
                    continue;
                }
                var r = ApplyRule(item, rule.Key, matchedData, action.Key, action.Parsers);
                if (r.Result)
                {
                    return r;
                }
            }
            return ApplyRule(item, All, matchedData, action.Key, action.Parsers);
        }

        private struct SemanticAction<T>
        {
            internal string Key;
            internal IDictionary<string, Func<string, ParseResult<T>>> Parsers;
        }


        void SetLevel(LogLevel level)
        {
            this.Level = level;
        }

        private static ParseResult<T> ApplyRule<T>(KeyValuePair<Semantic, string> item, string rule, string matchedData, string type, IDictionary<string, Func<string, ParseResult<T>>> parsers)
        {
            if (!item.Key.CastingRules.ContainsKey(rule))
            {
                return new ParseResult<T>();
            }
            var castingType = item.Key.CastingRules[rule];
            if (!castingType.Contains(type) || !parsers.ContainsKey(castingType))
            {
                return new ParseResult<T>();
            }
            return parsers[castingType](matchedData);
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