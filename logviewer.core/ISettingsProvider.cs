// Created by: egr
// Created at: 25.09.2013
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Drawing;
using Net.Sgoliver.NRtfTree.Util;

namespace logviewer.core
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
        ParsingTemplate ReadParsingTemplate(int index);
        void UpdateParsingProfile(ParsingTemplate template);
        void InsertParsingProfile(ParsingTemplate template);
        RtfCharFormat FormatHead(LogLevel level);
        RtfCharFormat FormatBody(LogLevel level);
        void UpdateColor(LogLevel level, Color color);
        Color ReadColor(LogLevel level);
        IDictionary<LogLevel, Color> DefaultColors { get; }
        int SelectedParsingTemplate { get; set; }
    }
}