// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 31.01.2015
// © 2012-2017 Alexander Egorov

using System.Collections.Generic;

namespace logviewer.engine.grammar
{
    /// <summary>
    /// Represents common pattern interface
    /// </summary>
    internal interface IPattern
    {
        string Compose(IList<Semantic> messageSchema);
    }
}