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
                : string.Format("{0} %  ({1}/second) remain {2}", this.Percent, this.Speed, this.RemainigToString());
        }

        private string RemainigToString()
        {
            if (this.Remainig.Days > 0)
            {
                return string.Format("{0} days {1} hours {2} minutes {3} seconds", this.Remainig.Days, this.Remainig.Hours,
                    this.Remainig.Minutes, this.Remainig.Seconds);
            }
            if (this.Remainig.Hours > 0)
            {
                return string.Format("{0} hours {1} minutes {2} seconds", this.Remainig.Hours, this.Remainig.Minutes, this.Remainig.Seconds);
            }
            if (this.Remainig.Minutes > 0)
            {
                return string.Format("{0} minutes {1} seconds", this.Remainig.Minutes, this.Remainig.Seconds);
            }
            return string.Format("{0} seconds", this.Remainig.Seconds);
        }
    }
}