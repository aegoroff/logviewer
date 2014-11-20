// Created by: egr
// Created at: 19.09.2012
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace logviewer.engine
{
    /// <summary>
    /// Represents parsed message structure with header, body and metadata if any
    /// </summary>
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
                        "yyyy-MM-ddTHH:mm:ss.FFFK",
                        "dd/MMM/yyyy:HH:mm:ssK",
                        "dd/MMM/yyyy:HH:mm:ss K",
                        "dd/MMM/yyyy:HH:mm:sszzz",
                        "dd/MMM/yyyy:HH:mm:ss zzz"
                    };

        /// <summary>
        /// Initializes new message instance using header and body specified
        /// </summary>
        /// <param name="header">Message header</param>
        /// <param name="body">Message body</param>
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

        /// <summary>
        /// Gets whether the message empty or not.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.head) && string.IsNullOrWhiteSpace(this.body) &&
                       this.bodyBuilder.Length == 0;
            }
        }

        /// <summary>
        /// Gets or sets message unique index to store it into database for example
        /// </summary>
        public long Ix
        {
            get { return this.ix; }
            set { this.ix = value; }
        }

        /// <summary>
        /// Gets message's head
        /// </summary>
        public string Header
        {
            get { return this.head ?? string.Empty; }
        }

        /// <summary>
        /// Gets message's body (without header)
        /// </summary>
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

        /// <summary>
        /// Gets whether the message has header
        /// </summary>
        public bool HasHeader
        {
            get { return this.properties != null; }
        }

        /// <summary>
        /// Add line to the message (the first line will be header the others will be considered as body)
        /// </summary>
        /// <param name="line">Line to add</param>
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

        /// <summary>
        /// Adds metadata into message
        /// </summary>
        /// <param name="parsedProperties">Metadata</param>
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

        static readonly IDictionary<string, LogLevel> levels = new Dictionary<string, LogLevel>(Levels(), StringComparer.OrdinalIgnoreCase);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        static IDictionary<string, LogLevel> Levels()
        {
            var r = new Dictionary<string, LogLevel>(StringComparer.OrdinalIgnoreCase);
            foreach (var n in Enum.GetNames(typeof(LogLevel)))
            {
                r.Add(n, (LogLevel)Enum.Parse(typeof(LogLevel), n));
            }
            r.Add("alert", LogLevel.Trace);
            r.Add("notice", LogLevel.Info);
            r.Add("critical", LogLevel.Error);
            r.Add("severe", LogLevel.Fatal);
            r.Add("emerg", LogLevel.Fatal);
            r.Add("emergency", LogLevel.Fatal);
            r.Add("warning", LogLevel.Warn);
            return r;
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        private static ParseResult<LogLevel> ParseLogLevel(string s)
        {
            LogLevel r;
            var success = levels.TryGetValue(s, out r);
            return new ParseResult<LogLevel> { Result = success, Value = r };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        private static ParseResult<DateTime> ParseDateTime(string s)
        {
            DateTime r;
            var success = DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out r);
            if (!success)
            {
                success = DateTime.TryParse(s, out r);
            }
            return new ParseResult<DateTime> { Result = success, Value = r };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        private static ParseResult<long> ParseInteger(string s)
        {
            long r;
            var success = long.TryParse(s, out r);
            return new ParseResult<long> { Result = success, Value = r };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        void ParseLogLevel(string dataToParse, ISet<Rule> rules, string property)
        {
            var result = RunSemanticAction(logLevelParsers, dataToParse, rules);
            if (result.Result)
            {
                this.integerProperties[property] = (int)result.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        void ParseDateTime(string dataToParse, ISet<Rule> rules, string property)
        {
            var result = RunSemanticAction(dateTimeParsers, dataToParse, rules);
            if (result.Result)
            {
                this.integerProperties[property] = result.Value.ToFileTime();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        void ParseInteger(string dataToParse, ISet<Rule> rules, string property)
        {
            var result = RunSemanticAction(integerParsers, dataToParse, rules);
            if (result.Result)
            {
                this.integerProperties[property] = result.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        private void ParseString(string dataToParse, ISet<Rule> rules, string property)
        {
            var result = RunSemanticAction(stringParsers, dataToParse, rules);
            if (result.Result)
            {
                this.stringProperties[property] = result.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        private void ApplySemanticRules(IDictionary<SemanticProperty, ISet<Rule>> schema)
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
                var sp = schema.First(p => p.Key == property.Key).Key;
                var rules = schema[property.Key];
                var matchedData = property.Value;

                switch (sp.Parser)
                {
                    case ParserType.LogLevel:
                        this.ParseLogLevel(matchedData, rules, property.Key);
                        break;
                    case ParserType.Datetime:
                        this.ParseDateTime(matchedData, rules, property.Key);
                        break;
                    case ParserType.Interger:
                        this.ParseInteger(matchedData, rules, property.Key);
                        break;
                    default:
                        this.ParseString(matchedData, rules, property.Key);
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ParseResult<T> ApplyRule<T>(Rule rule, string matchedData, IDictionary<string, Func<string, ParseResult<T>>> parsers)
        {
            Func<string, ParseResult<T>> func;
            return parsers.TryGetValue(rule.Type, out func) ? func(matchedData) : new ParseResult<T>();
        }

        /// <summary>
        /// Gets message's integer metadata using property name specified
        /// </summary>
        /// <param name="property">Property to get data of</param>
        /// <returns>Property value</returns>
        public long IntegerProperty(string property)
        {
            return GetProperty(this.integerProperties, property);
        }

        /// <summary>
        /// Gets message's string metadata using property name specified
        /// </summary>
        /// <param name="property">Property to get data of</param>
        /// <returns>Property value</returns>
        public string StringProperty(string property)
        {
            return GetProperty(this.stringProperties, property);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T GetProperty<T>(IDictionary<string, T> dict, string property)
        {
            T result;
            dict.TryGetValue(property, out result);
            return result;
        }
        
        /// <summary>
        /// Updates or add integer property
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="value">Property value</param>
        public void UpdateIntegerProperty(string property, long value)
        {
            this.integerProperties[property] = value;
        }

        /// <summary>
        /// Updates or add string property
        /// </summary>
        /// <param name="property">Property name</param>
        /// <param name="value">Property value</param>
        public void UpdateStringProperty(string property, string value)
        {
            this.stringProperties[property] = value;
        }

        /// <summary>
        /// Builds message from lines array. All metadata will be extracted using schema specified
        /// </summary>
        /// <param name="schema">Message schema to extract metadata by</param>
        public void Cache(IDictionary<SemanticProperty, ISet<Rule>> schema)
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
            this.Clear();
        }

        /// <summary>
        /// Clears message's builder
        /// </summary>
        public void Clear()
        {
            this.bodyBuilder.Clear();
        }

        /// <summary>
        /// Creates new <see cref="LogMessage"/> instance
        /// </summary>
        /// <returns>new <see cref="LogMessage"/> instance</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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