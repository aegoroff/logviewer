// Created by: egr
// Created at: 25.09.2013
// © 2012-2013 Alexander Egorov

namespace logviewer.core
{
    public interface ISettingsProvider
    {
        int KeepLastNFiles { get; set; }
        string FullPathToDatabase { get; }
    }
}