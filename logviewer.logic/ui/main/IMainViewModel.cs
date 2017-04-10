// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 07.11.2015
// © 2012-2017 Alexander Egorov

using System;
using System.ComponentModel;
using logviewer.logic.Annotations;
using logviewer.logic.storage;

namespace logviewer.logic.ui.main
{
    public interface IMainViewModel : INotifyPropertyChanged, IUpdatable, IDisposable
    {
        int MinLevel { get; }

        int MaxLevel { get; }

        int SortingOrder { get; }

        [PublicAPI]
        int LogProgress { get; set; }

        [PublicAPI]
        string LogProgressText { get; set; }

        string LogPath { get; set; }

        string MessageFilter { get; }

        bool UseRegularExpressions { get; set; }

        ISettingsProvider SettingsProvider { get; }

        DateTime From { get; set; }

        DateTime To { get; set; }

        bool UiControlsEnabled { get; set; }

        bool IsTextFilterFocused { get; set; }

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

        IVirtualizingCollection<string> Datasource { get; }

        Range Visible { get; set; }

        string GithubAccount { get; }

        string GithubProject { get; }

        long MessageCount { get; set; }

        void UpdateCount();
    }
}
