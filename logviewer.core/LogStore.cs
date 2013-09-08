using System;
using System.Data.SQLite;
using System.IO;

namespace logviewer.core
{
    public sealed class LogStore : IDisposable
    {
        #region Constants and Fields

        private readonly SQLiteConnectionStringBuilder conString;
        private readonly string databasePath;
        private long index;

        #endregion

        #region Constructors and Destructors

        public LogStore(string databaseFilePath = null)
        {
            databasePath = databaseFilePath ?? Path.GetTempFileName();
            SQLiteConnection.CreateFile(databasePath);
            conString = new SQLiteConnectionStringBuilder { DataSource = databasePath };

            RunSqlQuery(command => command.ExecuteNonQuery(), @"
                        CREATE TABLE Log (
                                 Ix INTEGER NOT NULL,
                                 Header TEXT  NOT NULL,
                                 Body  TEXT,
                                 Level INTEGER NOT NULL
                        );
                    ");
        }

        #endregion

        #region Public Methods and Operators

        public void AddMessage(LogMessage message)
        {
            const string Cmd = @"INSERT INTO Log(Ix, Header, Body, Level) VALUES (@Ix, @Header, @Body, @Level)";
            RunSqlQuery(delegate(SQLiteCommand command)
            {
                command.Parameters.AddWithValue("@Ix", index++);
                command.Parameters.AddWithValue("@Header", message.Header);
                command.Parameters.AddWithValue("@Body", message.Body);
                command.Parameters.AddWithValue("@Level", (int)message.Level);
                command.ExecuteNonQuery();
            }, Cmd);
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

        private void RunSqlQuery(Action<SQLiteConnection> action)
        {
            var connection = new SQLiteConnection(conString.ToString());
            using (connection)
            {
                connection.Open();
                action(connection);
            }
        }

        private void RunSqlQuery(Action<SQLiteCommand> action, params string[] commands)
        {
            RunSqlQuery(connection =>
            {
                foreach (var command in commands)
                {
                    using (var sqLiteCommand = connection.CreateCommand())
                    {
                        sqLiteCommand.CommandText = command;
                        action(sqLiteCommand);
                    }
                }
            });
        }

        #endregion
    }
}