// Created by: egr
// Created at: 19.11.2014
// © 2012-2014 Alexander Egorov

using System.Collections.Generic;
using System.Linq;

namespace logviewer.engine
{
    /// <summary>
    ///     Buildes rules dictionary from schema
    /// </summary>
    public class RulesBuilder
    {
        private readonly IDictionary<SemanticProperty, ISet<Rule>> rules;

        private readonly IDictionary<string, ParserType> typesStorage = new Dictionary<string, ParserType>
        {
            { "LogLevel", ParserType.LogLevel },
            { "LogLevel.None", ParserType.LogLevel },
            { "LogLevel.Trace", ParserType.LogLevel },
            { "LogLevel.Debug", ParserType.LogLevel },
            { "LogLevel.Info", ParserType.LogLevel },
            { "LogLevel.Warn", ParserType.LogLevel },
            { "LogLevel.Error", ParserType.LogLevel },
            { "LogLevel.Fatal", ParserType.LogLevel },
            { "DateTime", ParserType.Datetime },
            { "int", ParserType.Interger },
            { "Int32", ParserType.Interger },
            { "long", ParserType.Interger },
            { "Int64", ParserType.Interger },
            { "string", ParserType.String },
            { "String", ParserType.String },
        };

        /// <summary>
        /// Initialize new builder instance
        /// </summary>
        /// <param name="schema">Message schema</param>
        public RulesBuilder(IEnumerable<Semantic> schema)
        {
            if (schema == null)
            {
                this.rules = new Dictionary<SemanticProperty, ISet<Rule>>();
                return;
            }
            var dictionary = new Dictionary<SemanticProperty, ISet<Rule>>();
            foreach (var semantic in schema)
            {
                var k = dictionary.ContainsKey(semantic.Property) ? semantic.Property + "_1" : semantic.Property;
                dictionary.Add(this.Create(k, semantic.CastingRules), semantic.CastingRules);
            }
            this.rules = dictionary;
        }

        /// <summary>
        /// Gets rules dictionary that has been built from the schema
        /// </summary>
        public IDictionary<SemanticProperty, ISet<Rule>> Rules
        {
            get { return this.rules; }
        }

        /// <summary>
        /// Defines parser type of the property specified
        /// </summary>
        /// <param name="property">Property to define parser type for</param>
        /// <returns>Parser type info</returns>
        public ParserType DefineParserType(SemanticProperty property)
        {
            ParserType result;
            return !this.typesStorage.TryGetValue(this.Rules[property].First().Type, out result) ? ParserType.String : result;
        }

        private SemanticProperty Create(string name, IEnumerable<Rule> castingRules)
        {
            foreach (var rule in castingRules)
            {
                ParserType parserType;
                if (!this.typesStorage.TryGetValue(rule.Type, out parserType))
                {
                    continue;
                }
                return new SemanticProperty(name, parserType);
            }
            return new SemanticProperty(name, ParserType.String);
        }
    }
}