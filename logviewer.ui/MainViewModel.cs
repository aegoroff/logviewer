// Created by: egr
// Created at: 12.09.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using logviewer.core;
using logviewer.ui.Properties;

namespace logviewer.ui
{
    public class MainViewModel
    {
        public static MainViewModel Current { get; } = new MainViewModel();

        private readonly SqliteSettingsProvider settingsProvider = new SqliteSettingsProvider(ConfigurationManager.AppSettings["SettingsDatabase"], 10, 2000);

        private MainViewModel()
        {
            this.Templates = new List<string>();
            var fromDb = this.settingsProvider.ReadAllParsingTemplates();
            this.Templates.AddRange(fromDb.Select(t => t.DisplayName));
            this.From = DateTime.MinValue;
            this.To = DateTime.MaxValue;
        }

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

        public List<string> Sorting { get; private set; } = new List<string>
        {
            Resources.SortAsc,
            Resources.SortDesc
        };

        public List<string> Templates { get; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }
    }
}