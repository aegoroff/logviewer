using System;
using System.IO;
using logviewer.logic.support;

namespace logviewer.logic.ui.main
{
    public class LogWatcher : IDisposable
    {
        private readonly Action<string> action;
        private FileSystemWatcher logWatch;

        public LogWatcher(Action<string> action)
        {
            this.action = action;
            this.logWatch = new FileSystemWatcher();
            this.logWatch.Changed += this.OnChangeLog;
            this.logWatch.NotifyFilter = NotifyFilters.Size;
        }

        public void WatchLogFile(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            var directory = Path.GetDirectoryName(path);
            this.logWatch.Path = string.IsNullOrWhiteSpace(directory) ? "." : directory;
            this.logWatch.Filter = Path.GetFileName(path);
            this.logWatch.EnableRaisingEvents = true;
        }

        private void OnChangeLog(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                Log.Instance.TraceFormatted("Log {0} change event: {1}", e.FullPath, e.ChangeType);
                this.action(e.FullPath);
            }
        }

        public void Dispose()
        {
            this.logWatch?.Dispose();
            this.logWatch = null;
        }
    }
}