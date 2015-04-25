// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

using System.Diagnostics;

namespace logviewer.engine
{
    [DebuggerDisplay("{Content}")]
    internal class NamedPattern : Pattern
    {
        internal string Property { get; private set; }

        internal NamedPattern(string property, string content)
            : base(string.Format(@"(?<{0}>{1})", property, content))
        {
            this.Property = property;
        }
    }
}