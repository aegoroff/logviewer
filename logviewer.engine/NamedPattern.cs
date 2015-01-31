// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.engine
{
    internal class NamedPattern : Pattern
    {
        internal NamedPattern(string property, string content)
        {
            this.SetContent(string.Format(@"(?<{0}>{1})", property, content));
        }
    }
}