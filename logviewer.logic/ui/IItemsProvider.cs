// Created by: egr
// Created at: 10.11.2015
// © 2012-2015 Alexander Egorov

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
        long FetchCount();

        /// <summary>
        ///     Fetches a range of items.
        /// </summary>
        /// <param name="offset">The start index.</param>
        /// <param name="limit">The number of items to fetch.</param>
        /// <returns></returns>
        T[] FetchRange(long offset, int limit);
    }
}