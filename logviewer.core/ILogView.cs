// Created by: egr
// Created at: 19.09.2012
// © 2012-2014 Alexander Egorov

namespace logviewer.core
{
    public interface ILogView
    {
        string LogPath { get; set; }
        string LogFileName { get; }
        string HumanReadableLogSize { get; set; }
        string LogInfo { get; set; }
        void ClearRecentFilesList();
        void CreateRecentFileItem(string file);
        bool OpenLogFile();
        void ReadLog();
        void OnFailureRead(string errorMessage);
        bool OpenExport(string path);
        void SaveRtf();
        void SetCurrentPage(int page);
        void SetPageSize(int size);
        void Initialize();
        void FocusOnTextFilterControl();
        void SetProgress(LoadProgress progress);
        void SetLogProgressCustomText(string text);
        void StartLoadingLog(string path);
        void SetLoadedFileCapltion(string path);
        void SetFileEncoding(string encoding);
        void StartReading();
        void AddFilterItems(string[] items);
    }
}