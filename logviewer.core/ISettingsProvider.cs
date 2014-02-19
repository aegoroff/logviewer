// Created by: egr
// Created at: 25.09.2013
// © 2012-2013 Alexander Egorov

using System.Collections.Generic;

namespace logviewer.core
{
    public interface ISettingsProvider
    {
        int KeepLastNFiles { get; set; }
        string FullPathToDatabase { get; }
        string MessageFilter { get; set; }
        bool OpenLastFile { get; set; }
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
    }
}