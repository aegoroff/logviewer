// Created by: egr
// Created at: 20.09.2013
// © 2012-2013 Alexander Egorov

using System;

namespace logviewer.core
{
    public class LogReadCompletedEventArgs : EventArgs
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="rtf"></param>
        public LogReadCompletedEventArgs(string rtf)
        {
            this.Rtf = rtf;
        }

        public string Rtf { get; private set; }
    }
}