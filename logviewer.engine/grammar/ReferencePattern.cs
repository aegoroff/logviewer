// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 29.05.2015
// © 2012-2017 Alexander Egorov

using System.Collections.Generic;

namespace logviewer.engine.grammar
{
    internal class ReferencePattern : IPattern
    {
        private readonly IDictionary<string, IPattern> definitions;

        private readonly string grok;

        internal ReferencePattern(string grok, IDictionary<string, IPattern> definitions)
        {
            this.grok = grok;
            this.definitions = definitions;
        }

        internal string Property { get; set; }

        internal Semantic Schema { get; set; }

        public string Compose(IList<Semantic> messageSchema)
        {
            var pattern = this.definitions.ContainsKey(this.grok)
                              ? this.definitions[this.grok]
                              : new PassthroughPattern(this.grok);

            if (this.Schema != default(Semantic))
            {
                messageSchema.Add(this.Schema);
            }

            var result = pattern.Compose(messageSchema);
            return string.IsNullOrWhiteSpace(this.Property)
                       ? result
                       : $@"(?<{this.Property}>{result})";
        }
    }
}
