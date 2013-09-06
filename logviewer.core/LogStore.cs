using System;
using System.Data.SQLite;
using System.IO;

namespace logviewer.core
{
    public sealed class LogStore : IDisposable
    {
        #region Constants and Fields

        private readonly string databasePath;

        #endregion

        #region Constructors and Destructors

        public LogStore()
        {
            databasePath = Path.GetTempFileName();
            SQLiteConnection.CreateFile(databasePath);
        }

        #endregion

        #region Public Methods and Operators

        public void AddMessage(LogMessage message)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Methods

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                }
            }
        }

        #endregion
    }
}