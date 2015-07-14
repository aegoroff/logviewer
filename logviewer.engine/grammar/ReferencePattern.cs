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
                var pattern = this.definitions.ContainsKey(this.grok)
                    ? this.definitions[this.grok]
                    : new PassthroughPattern(this.grok);
                this.schema.AddRange(pattern.Schema);
                if (!string.IsNullOrWhiteSpace(this.Property))
                {
                    pattern = new NamedPattern(this.Property, pattern);
                }
                return pattern.Content;
            }
        }

        internal string Property { get; set; }

        public IList<Semantic> Schema
        {
            get { return this.schema; }
        }
    }
}