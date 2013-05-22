using System.IO;

namespace logviewer.core
{
    public interface IMainController
    {
        string HumanReadableLogSize { get; }
        long LogSize { get; }
        bool RebuildMessages { get; set; }
        int MessagesCount { get; }
        int TotalMessages { get; }
        int CurrentPage { get; set; }
        int TotalPages { get; }
        int DisplayedMessages { get; }
        void InitializeLogger();
        void LoadLog(string path);
        string ReadLog();
        string ReadLog(Stream stream);
        void CancelReading();
        void ClearCache();
        void MinFilter(int value);
        void MaxFilter(int value);
        void TextFilter(string value);
        void Ordering(bool reverse);
        void ReadRecentFiles();
        void SaveRecentFiles();
        void OpenLogFile();
        void ExportToRtf();
        int CountMessages(LogLevel level);
        void SetView(ILogView view);
        void SetPageSize();
    }
}