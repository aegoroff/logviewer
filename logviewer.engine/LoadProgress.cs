// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 20.09.2013
// © 2012-2018 Alexander Egorov

using System;

namespace logviewer.engine
{
    /// <summary>
    /// Represents log reading progress info
    /// </summary>
    public struct LoadProgress
    {
        /// <summary>
        /// Log reading speed
        /// </summary>
        public FileSize Speed { get; set; }

        /// <summary>
        /// Remaining time
        /// </summary>
        public TimeSpan Remaining { get; set; }

        /// <summary>
        /// Log read percent
        /// </summary>
        public int Percent { get; set; }
    }
}
