// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Diagnostics;

namespace logviewer.engine
{
    [DebuggerDisplay("{Content}")]
    internal class NamedPattern : IPattern
    {
        private readonly IPattern wrapped;

        internal NamedPattern(string property, IPattern wrapped)
        {
            this.wrapped = wrapped;
            this.Property = property;
        }

        internal string Property { get; private set; }

        public string Content
        {
            get { return string.Format(@"(?<{0}>{1})", this.Property, this.wrapped.Content); }
        }
    }
}