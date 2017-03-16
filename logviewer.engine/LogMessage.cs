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
        /// <remarks>For performance reasons the real type of extractedProperties must be KeyValuePair&lt;string, string&gt;[] otherwise parsing will not work</remarks>
        public void AddProperties(IEnumerable<KeyValuePair<string, string>> extractedProperties)
        {
            this.rawProperties = extractedProperties as KeyValuePair<string, string>[];
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        private static bool TryParseLogLevel(string str, out LogLevel level)
        {
            if (string.Equals(str, @"TRACE", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Trace;
                return true;
            }
            else if (string.Equals(str, @"DEBUG", StringComparison.OrdinalIgnoreCase) || string.Equals(str, @"DEBUGGING", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Debug;
                return true;
            }
            else if (string.Equals(str, @"INFO", StringComparison.OrdinalIgnoreCase) || string.Equals(str, @"NOTICE", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(str, @"INFORMATIONAL", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Info;
                return true;
            }
            else if (string.Equals(str, @"WARN", StringComparison.OrdinalIgnoreCase) || string.Equals(str, @"WARNING", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Warn;
                return true;
            }
            else if (string.Equals(str, @"ERROR", StringComparison.OrdinalIgnoreCase) || string.Equals(str, @"ERR", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(str, @"CRITICAL", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Error;
                return true;
            }
            else if (string.Equals(str, @"FATAL", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(str, @"SEVERE", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(str, @"EMERG", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(str, @"EMERGENCY", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(str, @"PANIC", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(str, @"ALERT", StringComparison.OrdinalIgnoreCase))
            {
                level = LogLevel.Fatal;
                return true;
            }
            else
            {
                level = LogLevel.None;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseLogLevel(string dataToParse, ICollection<GrokRule> rules, string property)
        {
            var result = rules.Count > 1
                ? TryRunSemanticAction(dataToParse, rules, out LogLevel level) 
                : TryParseLogLevel(dataToParse, out level);
            if (result)
            {
                this.integerProperties[property] = (int)level;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ParseDateTime(string dataToParse, string property)
        {
            var success = DateTime.TryParseExact(dataToParse, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None | DateTimeStyles.AssumeUniversal, out DateTime r);
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
            if (long.TryParse(dataToParse, out long r))
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

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < this.rawProperties.Length; i++)
            {
                var property = this.rawProperties[i];
                var key = property.Key;

                if (!schema.TryGetValue(key, out ISet<GrokRule> rules))
                {
                    continue;
                }

                var semanticProperty = default(SemanticProperty);

                foreach (var prop in schema)
                {
                    if (prop.Key != key)
                    {
                        continue;
                    }
                    semanticProperty = prop.Key;
                    break;
                }

                switch (semanticProperty.Parser)
                {
                    case ParserType.LogLevel:
                        this.ParseLogLevel(property.Value, rules, key);
                        break;
                    case ParserType.Datetime:
                        this.ParseDateTime(property.Value, key);
                        break;
                    case ParserType.Interger:
                        this.ParseInteger(property.Value, key);
                        break;
                    case ParserType.String:
                        this.ParseString(property.Value, key);
                        break;
                    default:
                        this.ParseString(property.Value, key);
                        break;
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
            dict.TryGetValue(property, out T result);
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
            var length = this.bodyBuilder.Length;
            if (length > 0)
            {
                this.bodyBuilder.Remove(length - 1, 1);
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
        private KeyValuePair<string, string>[] rawProperties;
        private long ix;

        #endregion
    }
}