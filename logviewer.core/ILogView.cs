// Created by: egr
// Created at: 19.09.2012
// © 2012-2013 Alexander Egorov

namespace logviewer.core
{
    public interface ILogView
    {
        string LogPath { get; set; }
        string LogFileName { get; }
        string HumanReadableLogSize { get; set; }
        string LogInfo { get; set; }
        string LogInfoFormatString { get; }
        void ClearRecentFilesList();
        void CreateRecentFileItem(string file);
        bool OpenLogFile();
        void ReadLog();
        void OnSuccessRead(string rtf);
        void OnFailureRead(string rtf);
        bool OpenExport(string path);
        void SetLoadedFileCapltion(string path);
        void SaveRtf();
        void SetCurrentPage(int page);
        void DisableForward(bool disabled);
        void DisableBack(bool disabled);
        void SetPageSize(int size);
        void Initialize();
        void FocusOnTextFilterControl();
        void SetProgress(LoadProgress progress);
        void StartLoadingLog(string path);
    }
}