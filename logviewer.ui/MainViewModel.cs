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
        private readonly SqliteSettingsProvider settingsProvider =
            new SqliteSettingsProvider(ConfigurationManager.AppSettings["SettingsDatabase"], 10, 2000);

        private MainViewModel()
        {
            this.Templates = new List<string>();
            var fromDb = this.settingsProvider.ReadAllParsingTemplates();
            this.Templates.AddRange(fromDb.Select(t => t.DisplayName));
            this.From = DateTime.MinValue;
            this.To = DateTime.MaxValue;
        }

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

        public List<string> Sorting { get; private set; } = new List<string>
        {
            Resources.SortDesc,
            Resources.SortAsc
        };

        public List<string> Templates { get; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public string MessageFilter
        {
            get { return this.settingsProvider.MessageFilter; }
            set { this.settingsProvider.MessageFilter = value; }
        }

        public bool UseRegularExpressions
        {
            get { return this.settingsProvider.UseRegexp; }
            set { this.settingsProvider.UseRegexp = value; }
        }

        public int MinLevel
        {
            get { return this.settingsProvider.MinLevel; }
            set { this.settingsProvider.MinLevel = value; }
        }

        public int MaxLevel
        {
            get { return this.settingsProvider.MaxLevel; }
            set { this.settingsProvider.MaxLevel = value; }
        }

        public int SelectedParsingTemplate
        {
            get { return this.settingsProvider.SelectedParsingTemplate; }
            set { this.settingsProvider.SelectedParsingTemplate = value; }
        }

        public int SortingOrder
        {
            get { return this.settingsProvider.Sorting ? 0 : 1; }
            set { this.settingsProvider.Sorting = value == 0; }
        }
    }
}