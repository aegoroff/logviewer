namespace logviewer.core
{
	public interface ILogView
	{
        string LogPath { get; set; }
        string LogFileName { get; }
		bool IsBusy { get; }
        bool CancellationPending { get; }
	    void ClearRecentFilesList();
	    void CreateRecentFileItem(string file);
	    bool OpenLogFile();
	    void ReadLog();
        void CancelRead();
        void LoadLog(string path);
	}
}