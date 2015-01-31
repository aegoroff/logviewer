// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.engine
{
    internal class PassthroughPattern : Pattern
    {
        internal PassthroughPattern(string content)
        {
            this.SetContent(GrokVisitor.PatternStart + content + GrokVisitor.PatternStop);
        }
    }
}