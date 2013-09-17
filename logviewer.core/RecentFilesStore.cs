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

        public RecentFilesStore(string settingsDatabase, int maxFiles)
        {
            this.maxFiles = maxFiles;
            this.connection = new DatabaseConnection(settingsDatabase);
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
            var result = 0L;
            const string CountFiles = @"SELECT count(1) FROM RecentFiles WHERE Path = @Path";
            this.connection.RunSqlQuery(delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Path", file);
                result = (long)command.ExecuteScalar();
            }, CountFiles);

            if (result > 0)
            {
                const string Update = @"Update RecentFiles SET OpenedAt = @OpenedAt WHERE Path = @Path";
                this.connection.RunSqlQuery(delegate(IDbCommand command)
                {
                    DatabaseConnection.AddParameter(command, "@Path", file);
                    DatabaseConnection.AddParameter(command, "@OpenedAt", DateTime.Now.Ticks);
                    command.ExecuteNonQuery();
                }, Update);
                
                return;
            }

            const string Cmd = @"INSERT INTO RecentFiles(Path, OpenedAt) VALUES (@Path, @OpenedAt)";
            this.connection.RunSqlQuery(delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Path", file);
                DatabaseConnection.AddParameter(command, "@OpenedAt", DateTime.Now.Ticks);
                command.ExecuteNonQuery();
            }, Cmd);

            this.connection.RunSqlQuery(delegate(IDbCommand command)
            {
                result = (long)command.ExecuteScalar();
            }, @"SELECT count(1) FROM RecentFiles");
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

        public IEnumerable<string> ReadFiles()
        {
            var result = new List<string>(maxFiles);
            this.connection.RunSqlQuery(delegate(IDbCommand command)
            {
                var rdr = command.ExecuteReader();
                using (rdr)
                {
                    while (rdr.Read())
                    {
                        result.Add(rdr[0] as string);
                    }
                }
            }, @"SELECT Path FROM RecentFiles ORDER BY OpenedAt DESC");
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