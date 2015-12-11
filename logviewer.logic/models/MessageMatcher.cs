// Created by: egr
// Created at: 10.12.2015
// © 2012-2015 Alexander Egorov

using System.Text.RegularExpressions;
using logviewer.engine;

namespace logviewer.logic.models
{
    /// <summary>
    ///     Represents prepared matcher structure
    /// </summary>
    public class MessageMatcher : IMessageMatcher
    {
        /// <summary>
        ///     Initializes new matcher instance using template specified
        /// </summary>
        /// <param name="template"></param>
        /// <param name="options"></param>
        public MessageMatcher(ParsingTemplate template, RegexOptions options)
        {
            if (template.IsEmpty)
            {
                return;
            }
            this.IncludeMatcher = new GrokMatcher(template.StartMessage,
                template.Compiled ? options | RegexOptions.Compiled : options);
            this.ExcludeMatcher = string.IsNullOrWhiteSpace(template.Filter) ? null : new GrokMatcher(template.Filter);
        }

        public GrokMatcher IncludeMatcher { get; }
        public GrokMatcher ExcludeMatcher { get; }
    }
}