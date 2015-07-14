// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

using System.Diagnostics;

namespace logviewer.engine.grammar
{
    [DebuggerDisplay("{Content}")]
    internal class NamedPattern : Pattern
    {
        internal NamedPattern(string property, IPattern wrapped)
            : base(string.Format(@"(?<{0}>{1})", property, wrapped.Content))
        {
        }
    }
}