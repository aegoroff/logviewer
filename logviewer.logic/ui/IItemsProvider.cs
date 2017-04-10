// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 10.11.2015
// © 2012-2017 Alexander Egorov

using logviewer.logic.Annotations;

namespace logviewer.logic.ui
{
    /// <summary>
    ///     Represents a provider of collection details.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    public interface IItemsProvider<out T>
    {
        /// <summary>
        ///     Fetches the total number of items available.
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        long FetchCount();

        /// <summary>
        ///     Fetches a range of items.
        /// </summary>
        /// <param name="offset">The start index.</param>
        /// <param name="limit">The number of items to fetch.</param>
        /// <returns></returns>
        [PublicAPI]
        T[] FetchRange(long offset, int limit);
    }
}
