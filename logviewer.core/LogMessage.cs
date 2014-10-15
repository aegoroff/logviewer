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
            this.ix = 0;
            this.properties = null;
            this.bodyBuilder = null;
            this.integerProperties = new Dictionary<string, long>();
            this.stringProperties = new Dictionary<string, string>();
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

        public bool HasHeader
        {
            get { return this.properties != null; }
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

        public void AddProperties(IDictionary<string, string> parsedProperties)
        {
            this.properties = parsedProperties;
        }

        private static readonly IDictionary<string, Func<string, ParseResult<LogLevel>>> logLevelParsers = new Dictionary
            <string, Func<string, ParseResult<LogLevel>>>
        {
            { "LogLevel", ParseLogLevel },
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
                "DateTime", ParseDateTime
            },
        };

        private static readonly IDictionary<string, Func<string, ParseResult<long>>> integerParsers = new Dictionary
            <string, Func<string, ParseResult<long>>>
        {
            { "long", ParseInteger },
            { "int", ParseInteger },
            { "Int32", ParseInteger },
            { "Int64", ParseInteger },
        };

        private static readonly IDictionary<string, Func<string, ParseResult<string>>> stringParsers = new Dictionary
            <string, Func<string, ParseResult<string>>>
        {
            {"string", s => new ParseResult<string> {Result = true, Value = s}},
            {"String", s => new ParseResult<string> {Result = true, Value = s}}
        };

        private static ParseResult<LogLevel> ParseLogLevel(string s)
        {
            LogLevel r;
            var success = Enum.TryParse(s, true, out r) && Enum.IsDefined(typeof(LogLevel), r);
            return new ParseResult<LogLevel> { Result = success, Value = r };
        }
        
        private static ParseResult<DateTime> ParseDateTime(string s)
        {
            DateTime r;
            var success = DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out r);
            return new ParseResult<DateTime> { Result = success, Value = r };
        }
        
        private static ParseResult<long> ParseInteger(string s)
        {
            long r;
            var success = long.TryParse(s, out r);
            return new ParseResult<long> { Result = success, Value = r };
        }

        bool ParseLogLevel(string dataToParse, ISet<Rule> rules, string property)
        {
            var result = RunSemanticAction(logLevelParsers, dataToParse, rules);
            if (result.Result)
            {
                this.integerProperties[property] = (int)result.Value;
            }
            return result.Result;
        }

        bool ParseDateTime(string dataToParse, ISet<Rule> rules, string property)
        {
            var result = RunSemanticAction(dateTimeParsers, dataToParse, rules);
            if (result.Result)
            {
                this.integerProperties[property] = result.Value.ToFileTime();
            }
            return result.Result;
        }

        bool ParseInteger(string dataToParse, ISet<Rule> rules, string property)
        {
            var result = RunSemanticAction(integerParsers, dataToParse, rules);
            if (result.Result)
            {
                this.integerProperties[property] = result.Value;
            }
            return result.Result;
        }

        private void ParseString(string dataToParse, ISet<Rule> rules, string property)
        {
            var result = RunSemanticAction(stringParsers, dataToParse, rules);
            if (result.Result)
            {
                this.stringProperties[property] = result.Value;
            }
        }

        private void ApplySemanticRules(IDictionary<string, ISet<Rule>> schema)
        {
            if (this.properties == null || schema == null)
            {
                return;
            }
            foreach (var property in this.properties)
            {
                if (!schema.ContainsKey(property.Key))
                {
                    continue;
                }
                var rules = schema[property.Key];
                var matchedData = property.Value;
                if (this.ParseLogLevel(matchedData, rules, property.Key) || this.ParseDateTime(matchedData, rules, property.Key) || this.ParseInteger(matchedData, rules, property.Key))
                {
                    continue;
                }
                this.ParseString(matchedData, rules, property.Key);
            }
        }

        private static ParseResult<T> RunSemanticAction<T>(IDictionary<string, Func<string, ParseResult<T>>> parsers, string dataToParse, ISet<Rule> rules)
        {
            var defaultRule = new Rule(rules.First().Type);
            foreach (var rule in rules.Where(rule => rule == defaultRule || dataToParse.Contains(rule.Pattern)))
            {
                var r = ApplyRule(rule, dataToParse, parsers);
                if (r.Result)
                {
                    return r;
                }
            }
            return ApplyRule(defaultRule, dataToParse, parsers);
        }


        private static ParseResult<T> ApplyRule<T>(Rule rule, string matchedData, IDictionary<string, Func<string, ParseResult<T>>> parsers)
        {
            return parsers.ContainsKey(rule.Type) ? parsers[rule.Type](matchedData) : new ParseResult<T>();
        }

        public long IntegerProperty(string property)
        {
            long result;
            this.integerProperties.TryGetValue(property, out result);
            return result;
        }
        
        public string StringProperty(string property)
        {
            string result;
            this.stringProperties.TryGetValue(property, out result);
            return result;
        }
        
        public void UpdateIntegerProperty(string property, long value)
        {
            this.integerProperties[property] = value;
        }

        public void UpdateStringProperty(string property, string value)
        {
            this.stringProperties[property] = value;
        }

        public void Cache(IDictionary<string, ISet<Rule>> schema)
        {
            if (this.head != null && this.body != null)
            {
                return;
            }
            if (this.bodyBuilder.Length > 0)
            {
                this.bodyBuilder.Remove(this.bodyBuilder.Length - 1, 1);
            }
            this.ApplySemanticRules(schema);
            this.body = this.bodyBuilder.ToString();
            this.bodyBuilder.Clear();
        }

        public static LogMessage Create()
        {
            return new LogMessage
            {
                bodyBuilder = new StringBuilder(),
                integerProperties = new Dictionary<string, long>(5),
                stringProperties = new Dictionary<string, string>(5)
            };
        }

        #region Constants and Fields

        private IDictionary<string, long> integerProperties; 
        private IDictionary<string, string> stringProperties; 
        private string body;
        private StringBuilder bodyBuilder;
        private string head;
        private long ix;
        private IDictionary<string, string> properties;

        #endregion
    }
}