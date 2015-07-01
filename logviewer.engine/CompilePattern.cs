// Created by: egr
// Created at: 29.05.2015
// © 2012-2015 Alexander Egorov

using System;

namespace logviewer.engine
{
    internal class CompilePattern : IPattern
    {
        private readonly string grok;
        private readonly Func<string, string> callback;

        internal CompilePattern(string grok, Func<string, string> callback)
        {
            this.grok = grok;
            this.callback = callback;
        }

        public string Content
        {
            get { return this.callback(this.grok); }
        }
    }
}