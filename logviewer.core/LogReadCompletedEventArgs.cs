// Created by: egr
// Created at: 20.09.2013
// © 2012-2013 Alexander Egorov

using System;

namespace logviewer.core
{
    public class LogReadCompletedEventArgs : EventArgs
    {
        private readonly string rtf;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="rtf"></param>
        /// <param name="elapsed"></param>
        public LogReadCompletedEventArgs(string rtf, TimeSpan elapsed)
        {
            this.rtf = rtf;
            this.Elapsed = elapsed;
        }

        public string Rtf
        {
            get { return this.rtf; }
        }

        public TimeSpan Elapsed { get; private set; }
    }
}