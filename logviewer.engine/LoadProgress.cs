// Created by: egr
// Created at: 20.09.2013
// © 2012-2014 Alexander Egorov

using System;

namespace logviewer.engine
{
    public struct LoadProgress
    {
        public FileSize Speed { get; set; }
        public TimeSpan Remainig { get; set; }
        public int Percent { get; set; }

        public static LoadProgress FromPercent(int percent)
        {
            return new LoadProgress { Percent = percent };
        }
    }
}