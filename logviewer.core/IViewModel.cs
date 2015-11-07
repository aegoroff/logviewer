// Created by: egr
// Created at: 07.11.2015
// © 2012-2015 Alexander Egorov

namespace logviewer.core
{
    public interface IViewModel
    {
        int MinLevel { get; }
        int MaxLevel { get; }
        int SortingOrder { get; }
        string LogPath { get; }
        string MessageFilter { get; }
        bool UseRegularExpressions { get; }
    }
}