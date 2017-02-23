// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 10.12.2015
// © 2012-2017 Alexander Egorov

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