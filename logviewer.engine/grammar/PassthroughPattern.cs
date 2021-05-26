// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 31.01.2015
// © 2012-2018 Alexander Egorov

using System.Diagnostics;

namespace logviewer.engine.grammar
{
    [DebuggerDisplay("{content}")]
    internal class PassthroughPattern : Pattern
    {
        private const string Start = "%{";

        private const string Stop = "}";

        internal PassthroughPattern(string content)
            : base(Start + content + Stop)
        {
        }
    }
}
