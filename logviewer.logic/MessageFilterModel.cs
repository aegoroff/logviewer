// Created by: egr
// Created at: 10.11.2015
// © 2012-2015 Alexander Egorov

using System;
using logviewer.engine;

namespace logviewer.logic
{
    public class MessageFilterModel
    {
        public MessageFilterModel()
        {
            this.Start = DateTime.MinValue;
            this.Finish = DateTime.MaxValue;
            this.Min = LogLevel.Trace;
            this.Max = LogLevel.Fatal;
        }

        public DateTime Start { get; set; }

        public DateTime Finish { get; set; }

        public LogLevel Min { get; set; }

        public LogLevel Max { get; set; }

        public string Filter { get; set; }

        public bool UseRegexp { get; set; }

        public bool ExcludeNoLevel { get; set; }

        public bool Reverse { get; set; }
    }
}