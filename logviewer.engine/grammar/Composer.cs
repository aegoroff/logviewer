// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace logviewer.engine.grammar
{
    [DebuggerDisplay("{Content}")]
    internal class Composer : List<IPattern>, IPattern
    {
        private readonly List<Semantic> schema = new List<Semantic>();

        public string Content
        {
            get { return string.Join(string.Empty, this.Select(pattern =>
            {
                this.schema.AddRange(pattern.Schema);
                return pattern.Content;
            })); }
        }

        public IList<Semantic> Schema
        {
            get { return this.schema; }
        }
    }
}