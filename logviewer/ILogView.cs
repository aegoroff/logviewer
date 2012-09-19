namespace logviewer
{
	public interface ILogView
	{
		string LogPath { get; }
	    void ClearRecentFilesList();
	    void CreateRecentFileItem(string file);
	}
}