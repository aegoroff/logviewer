// Created by: egr
// Created at: 19.09.2012
// © 2012-2015 Alexander Egorov

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
            var success = DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None | DateTimeStyles.AssumeUniversal, out r);
            if (!success)
            {
                success = DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None | DateTimeStyles.AssumeUniversal, out r);
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
        void ParseLogLevel(string dataToParse, ISet<GrokRule> rules, string property)
        {
            var result = rules.Count > 1
                ? RunSemanticAction(dataToParse, rules) 
                : ParseLogLevel(dataToParse);
            if (result.Result)
            {
                this.integerProperties[property] = (int)result.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        void ParseDateTime(string dataToParse, string property)
        {
            var result = ParseDateTime(dataToParse);
            if (result.Result)
            {
                this.integerProperties[property] = result.Value.ToFileTime();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        void ParseInteger(string dataToParse, string property)
        {
            var result = ParseInteger(dataToParse);
            if (result.Result)
            {
                this.integerProperties[property] = result.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        private void ParseString(string dataToParse, string property)
        {
            this.stringProperties[property] = dataToParse;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        private void ApplySemanticRules(IDictionary<SemanticProperty, ISet<GrokRule>> schema)
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
                        this.ParseDateTime(matchedData, property.Key);
                        break;
                    case ParserType.Interger:
                        this.ParseInteger(matchedData, property.Key);
                        break;
                    default:
                        this.ParseString(matchedData, property.Key);
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ParseResult<LogLevel> RunSemanticAction(string dataToParse, ISet<GrokRule> rules)
        {
            var result = new ParseResult<LogLevel> { Result = true };
            foreach (var rule in rules.Where(rule => dataToParse.Contains(rule.Pattern)))
            {
                result.Value = rule.Level;
                return result;
            }
            var defaultRule = rules.First(rule => rule.Pattern.Equals("*", StringComparison.OrdinalIgnoreCase));
            result.Value = defaultRule.Level;
            return result;
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
        public void Cache(IDictionary<SemanticProperty, ISet<GrokRule>> schema)
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