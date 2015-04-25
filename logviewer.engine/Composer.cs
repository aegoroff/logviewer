// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace logviewer.engine
{
    [DebuggerDisplay("{Content}")]
    internal class Composer : IPattern
    {
        private readonly List<IPattern> composition = new List<IPattern>();

        internal int Count
        {
            get { return this.composition.Count; }
        }

        public string Content
        {
            get { return string.Join(string.Empty, this.composition.Select(c => c.Content)); }
        }

        public void Add(IPattern pattern)
        {
            this.composition.Add(pattern);
        }

        internal string GetPattern(int ix)
        {
            return this.composition[ix].Content;
        }

        internal void SetPattern(int ix, IPattern pattern)
        {
            this.composition[ix] = pattern;
        }
    }
}