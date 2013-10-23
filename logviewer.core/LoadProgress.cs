// Created by: egr
// Created at: 20.09.2013
// © 2012-2013 Alexander Egorov

using System;
using logviewer.core.Properties;

namespace logviewer.core
{
    public struct LoadProgress
    {
        internal FileSize Speed { get; set; }
        internal TimeSpan Remainig { get; set; }
        public int Percent { get; set; }

        public static LoadProgress FromPercent(int percent)
        {
            return new LoadProgress { Percent = percent };
        }

        public override string ToString()
        {
            return this.Speed.Bytes == 0
                ? string.Format(Resources.SpeedPercent, this.Percent)
                : string.Format(Resources.SpeedPercentWithRemain, this.Percent, this.Speed, this.Remainig.TimespanToHumanString());
        }
    }
}