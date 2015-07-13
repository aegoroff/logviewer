// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

using System.Diagnostics;

namespace logviewer.engine.grammar
{
    [DebuggerDisplay("{Content}")]
    internal class Pattern : IPattern
    {
        internal Pattern(string c)
        {
            this.Content = c;
        }

        public string Content { get; private set; }
    }
}