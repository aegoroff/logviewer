// Created by: egr
// Created at: 12.09.2015
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using logviewer.core;
using logviewer.ui.Annotations;
using logviewer.ui.Properties;

namespace logviewer.ui
{
    public class MainViewModel : IViewModel
    {
        private readonly ISettingsProvider settingsProvider =
            new SqliteSettingsProvider(ConfigurationManager.AppSettings["SettingsDatabase"], 10, 2000);

        private const int PageTimeoutMilliseconds =10 * 1000;
        private const int PageSize = 200;

        private string logPath;
        private DateTime to;
        private DateTime from;
        private bool uiControlsEnabled;
        private string logStatistic;
        private string logSize;
        private string logEncoding;
        private int logProgress;
        private string logProgressText;
        private string totalMessages;
        private string toDisplayMessages;

        private MainViewModel()
        {
            var templates = this.CreateTemplateCommands();
            this.Templates = new ObservableCollection<TemplateCommand>(templates);
            this.Provider = new LogProvider(null, this.settingsProvider);
            this.Datasource = new AsyncVirtualizingCollection<string>(this.Provider, PageSize, PageTimeoutMilliseconds);

            this.From = DateTime.MinValue;
            this.To = DateTime.MaxValue;
            if (this.settingsProvider.OpenLastFile)
            {
                this.settingsProvider.UseRecentFilesStore(filesStore => this.LogPath = filesStore.ReadLastUsedItem());
            }
            this.uiControlsEnabled = true;
        }

        public IEnumerable<TemplateCommand> CreateTemplateCommands()
        {
            var fromDb = this.settingsProvider.ReadAllParsingTemplates();
            var ix = 0;
            foreach (var t in fromDb)
            {
                yield return new TemplateCommand
                {
                    Text = t.DisplayName,
                    Checked = ix == this.settingsProvider.SelectedParsingTemplate,
                    Index = ix
                };
                ++ix;
            }
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

        public ObservableCollection<TemplateCommand> Templates { get; }

        public DateTime From
        {
            get { return this.from; }
            set
            {
                this.from = value;
                this.OnPropertyChanged(nameof(this.From));
            }
        }

        public DateTime To
        {
            get { return this.to; }
            set
            {
                this.to = value;
                this.OnPropertyChanged(nameof(this.To));
            }
        }

        public bool UiControlsEnabled
        {
            get { return this.uiControlsEnabled; }
            set
            {
                this.uiControlsEnabled = value;
                this.OnPropertyChanged(nameof(this.UiControlsEnabled));
            }
        }

        public string TotalMessages
        {
            get { return this.totalMessages; }
            set
            {
                this.totalMessages = value;
                this.OnPropertyChanged(nameof(this.TotalMessages));
            }
        }

        public string ToDisplayMessages
        {
            get { return this.toDisplayMessages; }
            set
            {
                this.toDisplayMessages = value;
                this.OnPropertyChanged(nameof(this.toDisplayMessages));
            }
        }

        public string LogStatistic
        {
            get { return this.logStatistic; }
            set
            {
                this.logStatistic = value;
                this.OnPropertyChanged(nameof(this.LogStatistic));
            }
        }

        public string LogSize
        {
            get { return this.logSize; }
            set
            {
                this.logSize = value;
                this.OnPropertyChanged(nameof(this.LogSize));
            }
        }

        public string LogEncoding
        {
            get { return this.logEncoding; }
            set
            {
                this.logEncoding = value;
                this.OnPropertyChanged(nameof(this.LogEncoding));
            }
        }

        public LogProvider Provider { get; }

        public VirtualizingCollection<string> Datasource { get; }

        public int LogProgress
        {
            get { return this.logProgress; }
            set
            {
                this.logProgress = value;
                this.OnPropertyChanged(nameof(this.LogProgress));
            }
        }

        public string LogProgressText
        {
            get { return this.logProgressText; }
            set
            {
                this.logProgressText = value;
                this.OnPropertyChanged(nameof(this.LogProgressText));
            }
        }

        public string LogPath
        {
            get { return this.logPath; }
            set
            {
                this.logPath = value;
                this.settingsProvider.UseRecentFilesStore(s => s.Add(this.logPath));
                this.OnPropertyChanged(nameof(this.Caption));
            }
        }

        public string Caption => Resources.MainWindowCaptionPrefix +
                          (!string.IsNullOrWhiteSpace(this.logPath) ? ": " + this.logPath : string.Empty);

        public string MessageFilter
        {
            get { return this.settingsProvider.MessageFilter; }
            set
            {
                this.settingsProvider.MessageFilter = value;
                this.OnPropertyChanged(nameof(this.MessageFilter));
            }
        }

        public bool UseRegularExpressions
        {
            get { return this.settingsProvider.UseRegexp; }
            set
            {
                this.settingsProvider.UseRegexp = value;
                this.OnPropertyChanged(nameof(this.UseRegularExpressions));
            }
        }

        public int MinLevel
        {
            get { return this.settingsProvider.MinLevel; }
            set
            {
                this.settingsProvider.MinLevel = value;
                this.OnPropertyChanged(nameof(this.MinLevel));
            }
        }

        public int MaxLevel
        {
            get { return this.settingsProvider.MaxLevel; }
            set
            {
                this.settingsProvider.MaxLevel = value;
                this.OnPropertyChanged(nameof(this.MaxLevel));
            }
        }

        public int SelectedParsingTemplate
        {
            get { return this.settingsProvider.SelectedParsingTemplate; }
            set
            {
                this.settingsProvider.SelectedParsingTemplate = value;
                this.OnPropertyChanged(nameof(this.SelectedParsingTemplate));
            }
        }

        public int SortingOrder
        {
            get { return this.settingsProvider.Sorting ? 0 : 1; }
            set
            {
                this.settingsProvider.Sorting = value == 0;
                this.OnPropertyChanged(nameof(this.SortingOrder));
            }
        }

        public ISettingsProvider SettingsProvider => this.settingsProvider;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TemplateCommand
    {
        public string Text { get; set; }
        public bool Checked { get; set; }
        public int Index { get; set; }
    }
}