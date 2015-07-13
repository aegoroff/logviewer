// Created by: egr
// Created at: 29.05.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.engine.grammar
{
    internal class CompilePattern : IPattern
    {
        private readonly string grok;

        internal CompilePattern(string grok)
        {
            this.grok = grok;
        }

        public string Content
        {
            get { return this.grok; }
        }
    }
}