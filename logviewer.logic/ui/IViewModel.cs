// Created by: egr
// Created at: 07.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.ComponentModel;
using logviewer.logic.storage;
using logviewer.logic.support;

namespace logviewer.logic.ui
{
    public interface IViewModel : INotifyPropertyChanged, IUpdatable
    {
        int MinLevel { get; }
        int MaxLevel { get; }
        int SortingOrder { get; }
        int LogProgress { get; set; }
        string LogProgressText { get; set; }
        string LogPath { get; }
        string MessageFilter { get; }
        bool UseRegularExpressions { get; }
        ISettingsProvider SettingsProvider { get; }
        DateTime From { get; set; }
        DateTime To { get; set; }
        bool UiControlsEnabled { get; set; }
        string TotalMessages { get; set; }
        string ToDisplayMessages { get; set; }
        string LogStatistic { get; set; }
        string LogSize { get; set; }
        string LogEncoding { get; set; }
        LogProvider Provider { get; }
        VirtualizingCollection<string> Datasource { get; }
        Range Visible { get; set; }
        string GithubAccount { get; }
        string GithubProject { get; }
    }

    public struct Range
    {
        public int First;
        public int Last;
    }
}