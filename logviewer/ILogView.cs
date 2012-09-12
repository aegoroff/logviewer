namespace logviewer
{
	public interface ILogView
	{
		string LogPath { get; }
		void StartLongRunningDisplay();
		void StopLongRunningDisplay();
	}
}