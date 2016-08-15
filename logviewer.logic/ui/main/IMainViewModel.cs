// Created by: egr
// Created at: 07.11.2015
// © 2012-2016 Alexander Egorov

using System;
using System.ComponentModel;
using logviewer.logic.Annotations;
using logviewer.logic.storage;

namespace logviewer.logic.ui.main
{
    public interface IMainViewModel : INotifyPropertyChanged, IUpdatable
    {
        int MinLevel { get; }
        int MaxLevel { get; }
        int SortingOrder { get; }

        [PublicAPI]
        int LogProgress { get; set; }

        [PublicAPI]
        string LogProgressText { get; set; }

        string LogPath { get; }
        string MessageFilter { get; }
        bool UseRegularExpressions { get; }
        ISettingsProvider SettingsProvider { get; }
        DateTime From { get; set; }
        DateTime To { get; set; }
        bool UiControlsEnabled { get; set; }

        [PublicAPI]
        string TotalMessages { get; set; }

        [PublicAPI]
        string ToDisplayMessages { get; set; }

        [PublicAPI]
        string LogStatistic { get; set; }

        [PublicAPI]
        string LogSize { get; set; }

        [PublicAPI]
        string LogEncoding { get; set; }

        LogProvider Provider { get; }
        VirtualizingCollection<string> Datasource { get; }
        Range Visible { get; set; }
        string GithubAccount { get; }
        string GithubProject { get; }
        long MessageCount { get; set; }
    }

    public struct Range
    {
        public int First;
        public int Last;
    }
}