// Created by: egr
// Created at: 07.11.2015
// © 2012-2015 Alexander Egorov

using System;
using System.ComponentModel;

namespace logviewer.core
{
    public interface IViewModel : INotifyPropertyChanged
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
        int CurrentPage { get; set; }
        int TotalPages { get; set; }
        long MessagesCount { get; set; }
        string LogStatistic { get; set; }
        string LogSize { get; set; }
        string LogEncoding { get; set; }
    }
}