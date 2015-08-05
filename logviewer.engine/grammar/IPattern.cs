// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

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