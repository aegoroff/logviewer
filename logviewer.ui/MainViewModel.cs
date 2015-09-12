// Created by: egr
// Created at: 12.09.2015
// © 2012-2015 Alexander Egorov

using System.Collections.Generic;
using logviewer.ui.Properties;

namespace logviewer.ui
{
    public class MainViewModel
    {
        public static MainViewModel Current { get; } = new MainViewModel();

        public List<string> MinLevelLabeles { get; private set; } = new List<string>
        {
            Resources.TraceLabel,
            Resources.DebugLabel,
            Resources.InfoLabel,
            Resources.WarnLabel,
            Resources.ErrorLabel,
            Resources.FatalLabel
        };

        public List<string> MaxLevelLabeles { get; private set; } = new List<string>
        {
            Resources.TraceLabel,
            Resources.DebugLabel,
            Resources.InfoLabel,
            Resources.WarnLabel,
            Resources.ErrorLabel,
            Resources.FatalLabel
        };
    }
}