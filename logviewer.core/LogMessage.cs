// Created by: egr
// Created at: 19.09.2012
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace logviewer.core
{
    public struct LogMessage
    {
        private const char NewLine = '\n';

        static readonly string[] formats =
                    {
                        "yyyy-MM-dd HH:mm:ss,FFF",
                        "yyyy-MM-dd HH:mm:ss.FFF",
                        "yyyy-MM-dd HH:mm:ss,FFFK",
                        "yyyy-MM-dd HH:mm:ss.FFFK",
                        "yyyy-MM-dd HH:mm", 
                        "yyyy-MM-dd HH:mm:ss", 
                        "yyyy-MM-ddTHH:mm:ss,FFFK",
                        "yyyy-MM-ddTHH:mm:ss.FFFK"
                    };

        public LogMessage(string header, string body)
        {
            this.head = header;
            this.body = body;
            this.Level = LogLevel.None;
            this.ix = 0;
            this.semantic = null;
            this.bodyBuilder = null;
            this.Occured = DateTime.MinValue;
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
                    var success = Enum.TryParse(s, true, out r) && Enum.IsDefined(typeof(LogLevel), r);
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
                    var success = DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out r);
                    return new ParseResult<DateTime> { Result = success, Value = r };
                }
            },
        };

        private static readonly IDictionary<string, Func<string, ParseResult<long>>> integerParsers = new Dictionary
            <string, Func<string, ParseResult<long>>>
        {
            { "long", ParseInteger },
            { "int", ParseInteger },
        };

        private static ParseResult<long> ParseInteger(string s)
        {
            long r;
            var success = long.TryParse(s, out r);
            return new ParseResult<long> { Result = success, Value = r };
        }

        private static readonly IDictionary<string, Func<string, ParseResult<string>>> stringParsers = new Dictionary
            <string, Func<string, ParseResult<string>>>
        {
            { "string", s => new ParseResult<string> { Result = true, Value = s } }
        };

        private void ApplySemanticRules(LogMessageParseOptions options = LogMessageParseOptions.None)
        {
            if (this.semantic == null)
            {
                return;
            }
            if (options.HasFlag(LogMessageParseOptions.LogLevel))
            {
                var levelResult = this.RunSemanticAction(new SemanticAction<LogLevel> {Key = "LogLevel", Parsers = logLevelParsers});
                if (levelResult.Result)
                {
                    this.Level = levelResult.Value;
                }
            }
            if (options.HasFlag(LogMessageParseOptions.DateTime))
            {
                var dateResult = this.RunSemanticAction(new SemanticAction<DateTime> { Key = "DateTime", Parsers = dateTimeParsers });
                if (dateResult.Result)
                {
                    this.Occured = dateResult.Value;
                }
            }
            if (options.HasFlag(LogMessageParseOptions.Integer))
            {
                var integerResult = this.RunSemanticAction(new SemanticAction<long> { Key = "int", Parsers = integerParsers });
                if (integerResult.Result)
                {
                    // TODO: this.Occured = dateResult.Value;
                }
            }
            if (options.HasFlag(LogMessageParseOptions.String))
            {
                var stringResult = this.RunSemanticAction(new SemanticAction<string> { Key = "string", Parsers = stringParsers });
                if (stringResult.Result)
                {
                    // TODO: this.Occured = dateResult.Value;
                }
            }
        }

        private ParseResult<T> RunSemanticAction<T>(SemanticAction<T> action)
        {
// ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var item in this.semantic)
            {
                var r = ApplySemantic(item, action);
                if (r.Result)
                {
                    return r;
                }
            }
            return new ParseResult<T>();
        }

        private static ParseResult<T> ApplySemantic<T>(KeyValuePair<Semantic, string> semantic, SemanticAction<T> action)
        {
            var matchedData = semantic.Value;
            var defaultRule = new Rule(action.Key);
            foreach (var rule in semantic.Key.CastingRules.Where(rule => rule == defaultRule || matchedData.Contains(rule.Pattern)))
            {
                var r = ApplyRule(rule, matchedData, action);
                if (r.Result)
                {
                    return r;
                }
            }
            return ApplyRule(defaultRule, matchedData, action);
        }

        private struct SemanticAction<T>
        {
            internal string Key;
            internal IDictionary<string, Func<string, ParseResult<T>>> Parsers;
        }


        private static ParseResult<T> ApplyRule<T>(Rule rule, string matchedData, SemanticAction<T> action)
        {
            return action.Parsers.ContainsKey(rule.Type) ? action.Parsers[rule.Type](matchedData) : new ParseResult<T>();
        }

        public void Cache(LogMessageParseOptions options = LogMessageParseOptions.LogLevel)
        {
            if (this.head != null && this.body != null)
            {
                return;
            }
            if (this.bodyBuilder.Length > 0)
            {
                this.bodyBuilder.Remove(this.bodyBuilder.Length - 1, 1);
            }
            this.ApplySemanticRules(options);
            this.body = this.bodyBuilder.ToString();
            this.bodyBuilder.Clear();
        }

        public static LogMessage Create()
        {
            return new LogMessage { Level = LogLevel.None, bodyBuilder = new StringBuilder() };
        }

        #region Constants and Fields

        internal LogLevel Level;
        internal DateTime Occured;
        private string body;
        private StringBuilder bodyBuilder;
        private string head;
        private long ix;
        private IDictionary<Semantic, string> semantic;

        #endregion
    }
}