// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 31.01.2015
// © 2012-2017 Alexander Egorov

using System.Collections.Generic;
using System.Diagnostics;

namespace logviewer.engine.grammar
{
    [DebuggerDisplay("{" + nameof(content) + "}")]
    internal class Pattern : IPattern
    {
        private readonly string content;

        internal Pattern(string content) => this.content = content;

        public string Compose(IList<Semantic> messageSchema) => this.content;
    }
}
