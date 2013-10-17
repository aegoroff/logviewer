// Created by: egr
// Created at: 20.09.2013
// © 2012-2013 Alexander Egorov

using System;

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
                ? string.Format("{0} %", this.Percent)
                : string.Format("{0} %  ({1}/second) remain {2}", this.Percent, this.Speed, this.Remainig.TimespanToHumanString());
        }
    }
}