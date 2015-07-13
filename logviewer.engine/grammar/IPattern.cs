// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.engine.grammar
{
    /// <summary>
    /// Represents common pattern interface
    /// </summary>
    internal interface IPattern
    {
        /// <summary>
        /// Gets pattern's content
        /// </summary>
        string Content { get; }
    }
}