// Created by: egr
// Created at: 17.09.2013
// © 2012-2013 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Data;

namespace logviewer.core
{
    public sealed class RecentFilesStore : IDisposable
    {
        private readonly int maxFiles;
        private readonly DatabaseConnection connection;

        public RecentFilesStore(ISettingsProvider settings)
        {
            this.maxFiles = settings.KeepLastNFiles;
            this.connection = new DatabaseConnection(settings.FullPathToDatabase);
            this.CreateTables();
        }

        private void CreateTables()
        {
            const string CreateTable = @"
                        CREATE TABLE IF NOT EXISTS RecentFiles (
                                 Path TEXT PRIMARY KEY,
                                 OpenedAt INTEGER  NOT NULL
                        );
                    ";
            const string CreatePathIndex = @"CREATE INDEX IF NOT EXISTS IX_Path ON RecentFiles (Path)";
            this.connection.ExecuteNonQuery(CreateTable, CreatePathIndex);
        }

        public void Add(string file)
        {
            var result = this.connection.ExecuteScalar<long>(@"SELECT count(1) FROM RecentFiles WHERE Path = @Path", AddPath(file));

            Action<string> query = commandText => this.connection.ExecuteNonQuery(commandText, delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Path", file);
                DatabaseConnection.AddParameter(command, "@OpenedAt", DateTime.Now.Ticks);
            });

            if (result > 0)
            {
                query(@"Update RecentFiles SET OpenedAt = @OpenedAt WHERE Path = @Path");
                return;
            }

            query(@"INSERT INTO RecentFiles(Path, OpenedAt) VALUES (@Path, @OpenedAt)");
            
            result = this.connection.ExecuteScalar<long>(@"SELECT count(1) FROM RecentFiles");

            if (result <= maxFiles)
            {
                return;
            }
            const string DeleteTemplate =
                @"DELETE FROM RecentFiles 
                    WHERE OpenedAt IN (
                        SELECT OpenedAt FROM RecentFiles ORDER BY OpenedAt ASC LIMIT {0}
                )";
            var cmdDelete = string.Format(DeleteTemplate, result - maxFiles);
            this.connection.ExecuteNonQuery(cmdDelete);
        }

        private static Action<IDbCommand> AddPath(string file)
        {
            return cmd => DatabaseConnection.AddParameter(cmd, "@Path", file);
        }

        public void Remove(params string[] files)
        {
            const string Cmd = @"DELETE FROM RecentFiles WHERE Path = @Path";

            this.connection.BeginTran();
            foreach (var file in files)
            {
                this.connection.ExecuteNonQuery(Cmd, Remove(file));
            }
            this.connection.CommitTran();
        }

        private static Action<IDbCommand> Remove(string file)
        {
            return command => DatabaseConnection.AddParameter(command, "@Path", file);
        }

        public IEnumerable<string> ReadFiles()
        {
            var result = new List<string>(maxFiles);
            this.connection.ExecuteReader(reader => result.Add(reader[0] as string), @"SELECT Path FROM RecentFiles ORDER BY OpenedAt DESC");
            return result;
        }
        
        public string ReadLastOpenedFile()
        {
            var result = string.Empty;
            Action<IDataReader> onRead = delegate(IDataReader reader)
            {
                result = reader[0] as string;
            };
            this.connection.ExecuteReader(onRead, @"SELECT Path FROM RecentFiles ORDER BY OpenedAt DESC LIMIT 1");
            return result;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.connection.Dispose();
            }
        }
    }
}