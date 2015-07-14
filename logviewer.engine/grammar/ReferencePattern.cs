// Created by: egr
// Created at: 29.05.2015
// © 2012-2015 Alexander Egorov

using System.Collections.Generic;

namespace logviewer.engine.grammar
{
    internal class ReferencePattern : IPattern
    {
        private readonly string grok;
        private readonly IDictionary<string, IPattern> definitions;
        private readonly List<Semantic> schema = new List<Semantic>();

        internal ReferencePattern(string grok, IDictionary<string, IPattern> definitions)
        {
            this.grok = grok;
            this.definitions = definitions;
        }

        public string Content
        {
            get
            {
                var content = this.definitions.ContainsKey(this.grok)
                    ? this.definitions[this.grok].Content
                    : new PassthroughPattern(this.grok).Content;
                return string.IsNullOrWhiteSpace(this.Property) ? content : new NamedPattern(this.Property, content).Content;
            }
        }

        internal string Property { get; set; }

        internal List<Semantic> Schema
        {
            get { return this.schema; }
        }
    }
}