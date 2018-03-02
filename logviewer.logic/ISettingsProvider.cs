// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 25.09.2013
// © 2012-2018 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Drawing;
using logviewer.engine;
using logviewer.logic.models;
using logviewer.logic.storage;
using logviewer.logic.ui.main;
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

        IDictionary<LogLevel, Color> DefaultColors { get; }

        int SelectedParsingTemplate { get; set; }

        ISimpleOptionsStore SimpleOptionsStore { get; }

        ParsingTemplate ReadParsingTemplate();

        IList<string> ReadParsingTemplateList();

        IList<ParsingTemplate> ReadAllParsingTemplates();

        ParsingTemplate ReadParsingTemplate(int index);

        void UpdateParsingTemplate(ParsingTemplate template);

        void InsertParsingTemplate(ParsingTemplate template);

        void DeleteParsingTemplate(int ix);

        RtfCharFormat FormatHead(LogLevel level);

        RtfCharFormat FormatBody(LogLevel level);

        TextFormat GetFormat(LogLevel level);

        void UpdateColor(LogLevel level, Color color);

        Color ReadColor(LogLevel level);

        void ExecuteUsingRecentFilesStore(Action<IStringCollectionStore> action);

        T GetUsingRecentFilesStore<T>(Func<IStringCollectionStore, T> function);

        void ExecuteUsingRecentFiltersStore(Action<IStringCollectionStore> action);

        T GetUsingRecentFiltersStore<T>(Func<IStringCollectionStore, T> function);
    }
}
