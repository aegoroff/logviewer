// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

using System.Collections.Generic;
using System.Diagnostics;

namespace logviewer.engine.grammar
{
    [DebuggerDisplay("{Content}")]
    internal class Pattern : IPattern
    {
        private readonly List<Semantic> schema = new List<Semantic>();

        internal Pattern(string content)
        {
            this.Content = content;
        }

        public string Content { get; private set; }

        public IList<Semantic> Schema
        {
            get { return this.schema; }
        }
    }
}