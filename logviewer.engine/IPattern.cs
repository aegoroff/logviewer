// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.engine
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

        /// <summary>
        /// Adds new pattern into composition
        /// </summary>
        /// <param name="pattern">pattern to add</param>
        void Add(IPattern pattern);
    }
}