// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 19.09.2012
// © 2012-2017 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const int DefaultPropertyDicitionaryCapacity = 5;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local

        /// <summary>
        /// Initializes new message instance using header and body specified
        /// </summary>
        /// <param name="header">Message header</param>
        /// <param name="body">Message body</param>
        public LogMessage(string header, string body)
        {
            this.head = header;
            this.body = body;
            this.ix = 0L;
            this.rawProperties = null;
            this.bodyBuilder = null;
            this.integerProperties = new Dictionary<string, long>(DefaultPropertyDicitionaryCapacity);
            this.stringProperties = new Dictionary<string, string>(DefaultPropertyDicitionaryCapacity);
        }

        /// <summary>
        /// Gets whether the message empty or not.
        /// </summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(this.head) && string.IsNullOrWhiteSpace(this.body) &&
                               this.bodyBuilder.Length == 0;

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
        public string Header => this.head ?? string.Empty;

        /// <summary>
        /// Gets message's body (without header)
        /// </summary>
        public string Body => this.body ??
                              (this.bodyBuilder.Length == 1 && this.bodyBuilder[0] == NewLine
                                  ? string.Empty
                                  : this.bodyBuilder.ToString());

        /// <summary>
        /// Gets whether the message has header
        /// </summary>
        public bool HasHeader => this.rawProperties != null;

        /// <summary>
        /// All supportable dates formats to parse
        /// </summary>
        public static string[] Formats { get; } = {
            @"yyyy-MM-dd HH:mm:ss,FFF",
            @"yyyy-MM-dd HH:mm:ss.FFF",
            @"yyyy-MM-dd HH:mm:ss,FFFK",
            @"yyyy-MM-dd HH:mm:ss.FFFK",
            @"yyyy-MM-dd HH:mm",
            @"yyyy-MM-dd HH:mm:ss",
            @"yyyy-MM-ddTHH:mm:ss,FFFK",
            @"yyyy-MM-ddTHH:mm:ss.FFFK",
            @"dd/MMM/yyyy:HH:mm:ssK",
            @"dd/MMM/yyyy:HH:mm:ss K",
            @"dd/MMM/yyyy:HH:mm:sszzz",
            @"dd/MMM/yyyy:HH:mm:ss zzz"
        };

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
        /// <param name="extractedProperties">Concrete messate properties extracted by template</param>
        public void AddProperties(IDictionary<string, string> extractedProperties)
        {
            this.rawProperties = extractedProperties;
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        private static bool TryParseLogLevel(string s, out LogLevel level)
        {
            if (string.Equals(s, @"TRACE", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Trace;
                return true;
            }
            if (string.Equals(s, @"DEBUG", StringComparison.OrdinalIgnoreCase) || string.Equals(s, @"DEBUGGING", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Debug;
                return true;
            }
            if (string.Equals(s, @"INFO", StringComparison.OrdinalIgnoreCase) || string.Equals(s, @"NOTICE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s, @"INFORMATIONAL", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Info;
                return true;
            }
            if (string.Equals(s, @"WARN", StringComparison.OrdinalIgnoreCase) || string.Equals(s, @"WARNING", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Warn;
                return true;
            }
            if (string.Equals(s, @"ERROR", StringComparison.OrdinalIgnoreCase) || string.Equals(s, @"ERR", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s, @"CRITICAL", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Error;
                return true;
            }
            if (string.Equals(s, @"FATAL", StringComparison.OrdinalIgnoreCase) || string.Equals(s, @"SEVERE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s, @"EMERG", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s, @"EMERGENCY", StringComparison.OrdinalIgnoreCase) || string.Equals(s, @"PANIC", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(s, @"ALERT", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Fatal;
                return true;
            }
            level = LogLevel.None;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseLogLevel(string dataToParse, ICollection<GrokRule> rules, string property)
        {
            LogLevel level;
            var result = rules.Count > 1
                ? TryRunSemanticAction(dataToParse, rules, out level) 
                : TryParseLogLevel(dataToParse, out level);
            if (result)
            {
                this.integerProperties[property] = (int)level;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseDateTime(string dataToParse, string property)
        {
            DateTime r;
            var success = DateTime.TryParseExact(dataToParse, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None | DateTimeStyles.AssumeUniversal, out r);
            if (!success)
            {
                success = DateTime.TryParse(dataToParse, CultureInfo.InvariantCulture, DateTimeStyles.None | DateTimeStyles.AssumeUniversal, out r);
            }
            if (success)
            {
                this.integerProperties[property] = r.ToFileTime();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseInteger(string dataToParse, string property)
        {
            long r;
            var success = long.TryParse(dataToParse, out r);
            if (success)
            {
                this.integerProperties[property] = r;
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
            if (this.rawProperties == null || schema == null)
            {
                return;
            }

            // Ugly but allows to avoid on allocation per call
            // This code is rather performance critical
            var enumerator = this.rawProperties.GetEnumerator();
            using (enumerator)
            {
                while (enumerator.MoveNext())
                {
                    var property = enumerator.Current;
                    if (!schema.ContainsKey(property.Key))
                    {
                        continue;
                    }
                    var semanticProperty = default(SemanticProperty);

                    var schemaEnumarator = schema.GetEnumerator();
                    using (schemaEnumarator)
                    {
                        while (schemaEnumarator.MoveNext())
                        {
                            if (schemaEnumarator.Current.Key != property.Key)
                            {
                                continue;
                            }
                            semanticProperty = schemaEnumarator.Current.Key;
                            break;
                        }
                    }

                    var rules = schema[property.Key];
                    var matchedData = property.Value;

                    switch (semanticProperty.Parser)
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
                        case ParserType.String:
                            this.ParseString(matchedData, property.Key);
                            break;
                        default:
                            this.ParseString(matchedData, property.Key);
                            break;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryRunSemanticAction(string dataToParse, IEnumerable<GrokRule> rules, out LogLevel level) //-V3009
        {
            var enumerator = rules.GetEnumerator();
            using (enumerator)
            {
                while (enumerator.MoveNext())
                {
                    var rule = enumerator.Current;
                    if (!dataToParse.Contains(rule.Pattern))
                    {
                        continue;
                    }
                    level = rule.Level;
                    return true;
                }
                enumerator.Reset();
                level = LogLevel.None;
                while (enumerator.MoveNext())
                {
                    var rule = enumerator.Current;
                    if (rule.Pattern.Equals(GrokRule.DefaultPattern, StringComparison.OrdinalIgnoreCase))
                    {
                        level = rule.Level;
                    }
                }
            }
            return true;
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
            return new LogMessage(null, null)
            {
                bodyBuilder = new StringBuilder()
            };
        }

        #region Constants and Fields

        private Dictionary<string, long> integerProperties; 
        private Dictionary<string, string> stringProperties; 
        private string body;
        private StringBuilder bodyBuilder;
        private string head;
        private IDictionary<string, string> rawProperties;
        private long ix;

        #endregion
    }
}