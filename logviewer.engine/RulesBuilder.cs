// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 19.11.2014
// © 2012-2017 Alexander Egorov

using System.Collections.Generic;
using System.Linq;

namespace logviewer.engine
{
    /// <summary>
    ///     Buildes rules dictionary from schema
    /// </summary>
    public class RulesBuilder
    {
        private readonly IDictionary<SemanticProperty, ISet<GrokRule>> rules;

        /// <summary>
        /// Initialize new builder instance
        /// </summary>
        /// <param name="schema">Message schema</param>
        public RulesBuilder(IEnumerable<Semantic> schema)
        {
            if (schema == null)
            {
                this.rules = new Dictionary<SemanticProperty, ISet<GrokRule>>(new SemanticProperty());
                return;
            }
            this.rules = new Dictionary<SemanticProperty, ISet<GrokRule>>(new SemanticProperty());
            foreach (var semantic in schema)
            {
                var property = semantic.Property;
                var castingRules = semantic.CastingRules;
                var k = this.rules.ContainsKey(property) ? property + "_1" : property; // Not L10N
                this.rules.Add(Create(k, castingRules), castingRules);
            }
        }

        /// <summary>
        /// Gets rules dictionary that has been built from the schema
        /// </summary>
        public IDictionary<SemanticProperty, ISet<GrokRule>> Rules => this.rules;

        /// <summary>
        /// Defines parser type of the property specified
        /// </summary>
        /// <param name="property">Property to define parser type for</param>
        /// <returns>Parser type info</returns>
        public ParserType DefineParserType(SemanticProperty property)
        {
            return this.rules.ContainsKey(property) ? this.rules[property].First().Type : ParserType.String;
        }

        private static SemanticProperty Create(string name, IEnumerable<GrokRule> castingRules)
        {
            foreach (var rule in castingRules)
            {
                return new SemanticProperty(name, rule.Type);
            }
            return new SemanticProperty(name, ParserType.String);
        }
    }
}