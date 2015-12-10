// Created by: egr
// Created at: 10.12.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.engine
{
    /// <summary>
    /// Represents message matcher interface
    /// </summary>
    public interface IMessageMatcher
    {
        /// <summary>
        /// Matcher that defines message start
        /// </summary>
        GrokMatcher IncludeMatcher { get; }

        /// <summary>
        /// Matcher that filters row (row will not be included into any message)
        /// </summary>
        GrokMatcher ExcludeMatcher { get; }
    }
}