// Created by: egr
// Created at: 31.01.2015
// © 2012-2016 Alexander Egorov

using System.Collections.Generic;
using System.Diagnostics;

namespace logviewer.engine.grammar
{
    [DebuggerDisplay("{content}")]
    internal class Pattern : IPattern
    {
        private readonly string content;

        internal Pattern(string content)
        {
            this.content = content;
        }

        public string Compose(IList<Semantic> messageSchema)
        {
            return this.content;
        }
    }
}