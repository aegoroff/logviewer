// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 12.09.2015
// © 2012-2017 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows;
using logviewer.logic;
using logviewer.logic.storage;
using logviewer.logic.ui;
using logviewer.logic.ui.main;
using logviewer.logic.ui.update;
using logviewer.ui.Annotations;
using logviewer.ui.Properties;

namespace logviewer.ui
{
    public sealed class MainViewModel : IMainViewModel
    {
        private readonly ISettingsProvider settingsProvider =
            new SqliteSettingsProvider(ConfigurationManager.AppSettings["SettingsDatabase"], 2000, 10);

        private const int PageTimeoutMilliseconds = 10 * 1000;
        private const int PageSize = 128;

        private string logPath;
        private DateTime to;
        private DateTime from;
        private bool uiControlsEnabled;
        private bool isTextFilterFocused;
        private string logStatistic;
        private string logSize;
        private string logEncoding;
        private int logProgress;
        private string logProgressText;
        private string totalMessages;
        private string toDisplayMessages;
        private Range visible;

        private MainViewModel()
        {
            this.Templates = new ObservableCollection<TemplateCommandViewModel>(this.ReadParsingTemplateCommands());
            this.Provider = new LogProvider(null, this.settingsProvider);
            this.Datasource = new VirtualizingCollection<string>(this.Provider, PageSize, PageTimeoutMilliseconds);
            
            this.From = DateTime.MinValue;
            this.To = DateTime.MaxValue;
            if (this.settingsProvider.OpenLastFile)
            {
                this.LogPath = this.settingsProvider.GetUsingRecentFilesStore(store => store.ReadLastUsedItem());
            }

            this.uiControlsEnabled = true;
        }

        public IEnumerable<TemplateCommandViewModel> ReadParsingTemplateCommands()
        {
            return this.settingsProvider.ReadAllParsingTemplates().ToCommands(this.settingsProvider.SelectedParsingTemplate);
        }

        public static MainViewModel Current { get; } = new MainViewModel();

        public IEnumerable<string> MinLevelLabeles { get; } = new List<string>
        {
            Resources.TraceLabel,
            Resources.DebugLabel,
            Resources.InfoLabel,
            Resources.WarnLabel,
            Resources.ErrorLabel,
            Resources.FatalLabel
        };

        public IEnumerable<string> MaxLevelLabeles { get; } = new List<string>
        {
            Resources.TraceLabel,
            Resources.DebugLabel,
            Resources.InfoLabel,
            Resources.WarnLabel,
            Resources.ErrorLabel,
            Resources.FatalLabel
        };

        public IEnumerable<string> Sorting { get; } = new List<string>
        {
            Resources.SortDesc,
            Resources.SortAsc
        };

        public ObservableCollection<TemplateCommandViewModel> Templates { get; }

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

        public bool IsTextFilterFocused
        {
            get { return this.isTextFilterFocused; }
            set
            {
                this.isTextFilterFocused = value;
                this.OnPropertyChanged(nameof(this.IsTextFilterFocused));
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

        public IVirtualizingCollection<string> Datasource { get; }

        public Range Visible
        {
            get { return this.visible; }
            set
            {
                this.visible = value;
                this.OnPropertyChanged(nameof(this.Visible));
            }
        }

        public string GithubAccount => ConfigurationManager.AppSettings["GitHubAccount"];

        public string GithubProject => ConfigurationManager.AppSettings["GitHubProject"];

        public long MessageCount { get; set; }

        internal IWin32Window Window { get; set; }

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
                this.settingsProvider.ExecuteUsingRecentFilesStore(s => s.Add(this.logPath));
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
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ShowDialogAboutNewVersionAvaliable(Version current, Version latest, string targetAddress)
        {
            var model = new UpdateViewModel(current, latest, targetAddress);
            new Update(model).Show();
        }

        public void ShowNoUpdateAvaliable()
        {
            Xceed.Wpf.Toolkit.MessageBox.Show(Resources.NoUpdateAvailable, Resources.NoUpdateAvailableCaption, MessageBoxButton.OK);
        }

        public void UpdateCount()
        {
            this.Datasource.ChangeVisible(this.Visible);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Provider.Dispose();
            }
        }
    }
}