// Created by: egr
// Created at: 20.09.2013
// © 2012-2014 Alexander Egorov

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
        public TimeSpan Remainig { get; set; }
        
        /// <summary>
        /// Log read percent
        /// </summary>
        public int Percent { get; set; }

        /// <summary>
        /// Creates new <see cref="LoadProgress"/> instance using percent specified
        /// </summary>
        /// <param name="percent">Percent data</param>
        /// <returns>New <see cref="LoadProgress"/> instance</returns>
        public static LoadProgress FromPercent(int percent)
        {
            return new LoadProgress { Percent = percent };
        }
    }
}