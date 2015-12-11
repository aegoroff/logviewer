// Created by: egr
// Created at: 20.09.2013
// © 2012-2015 Alexander Egorov

using System;

namespace logviewer.logic.models
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