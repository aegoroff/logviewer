// Created by: egr
// Created at: 25.09.2013
// © 2012-2016 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Drawing;
using logviewer.engine;
using logviewer.logic.models;
using logviewer.logic.storage;
using Net.Sgoliver.NRtfTree.Util;

namespace logviewer.logic
{
    public interface ISettingsProvider
    {
        int KeepLastNFiles { get; set; }
        string FullPathToDatabase { get; }
        string MessageFilter { get; set; }
        DateTime LastUpdateCheckTime { get; set; }
        bool OpenLastFile { get; set; }
        bool AutoRefreshOnFileChange { get; set; }
        int MinLevel { get; set; }
        int MaxLevel { get; set; }
        int PageSize { get; set; }
        bool Sorting { get; set; }
        bool UseRegexp { get; set; }
        ParsingTemplate ReadParsingTemplate();
        IList<string> ReadParsingTemplateList();
        IList<ParsingTemplate> ReadAllParsingTemplates();
        ParsingTemplate ReadParsingTemplate(int index);
        void UpdateParsingTemplate(ParsingTemplate template);
        void InsertParsingTemplate(ParsingTemplate template);
        void DeleteParsingTemplate(int ix);
        RtfCharFormat FormatHead(LogLevel level);
        RtfCharFormat FormatBody(LogLevel level);
        void UpdateColor(LogLevel level, Color color);
        Color ReadColor(LogLevel level);
        IDictionary<LogLevel, Color> DefaultColors { get; }
        int SelectedParsingTemplate { get; set; }
        void UseRecentFilesStore(Action<RecentItemsStore> action);
        void UseRecentFiltersStore(Action<RecentItemsStore> action);
        IOptionsProvider OptionsProvider { get; }
    }
}