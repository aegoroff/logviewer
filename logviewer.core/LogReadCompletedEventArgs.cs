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
        public LogReadCompletedEventArgs(string rtf)
        {
            this.rtf = rtf;
        }

        public string Rtf
        {
            get { return this.rtf; }
        }
    }
}