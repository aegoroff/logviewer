// Created by: egr
// Created at: 31.01.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.engine
{
    internal abstract class Pattern : IPattern
    {
        protected void SetContent(string c)
        {
            this.Content = c;
        }

        public string Content { get; private set; }

        public void Add(IPattern pattern)
        {
            throw new System.NotImplementedException();
        }
    }
}