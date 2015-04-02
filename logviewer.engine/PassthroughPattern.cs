// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.engine
{
    internal class PassthroughPattern : Pattern
    {
        internal const string Start = "%{";
        private const string Stop = "}";

        internal PassthroughPattern(string content)
            : base(Start + content + Stop)
        {
        }
    }
}