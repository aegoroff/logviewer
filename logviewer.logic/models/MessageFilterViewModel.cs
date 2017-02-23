// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 10.11.2015
// © 2012-2017 Alexander Egorov

using System;
using logviewer.engine;
using logviewer.logic.Annotations;

namespace logviewer.logic.models
{
    public class MessageFilterViewModel
    {
        public MessageFilterViewModel()
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

        [PublicAPI]
        public bool ExcludeNoLevel { get; set; }

        public bool Reverse { get; set; }
    }
}