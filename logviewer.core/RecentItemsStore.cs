// Created by: egr
// Created at: 17.09.2013
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Data;

namespace logviewer.core
{
    public sealed class RecentItemsStore : IDisposable
    {
        private readonly string tableName;
        private readonly int maxItems;
        private readonly DatabaseConnection connection;

        public RecentItemsStore(ISettingsProvider settings, string tableName)
        {
            this.tableName = tableName;
            this.maxItems = settings.KeepLastNFiles;
            this.connection = new DatabaseConnection(settings.FullPathToDatabase);
            this.CreateTables();
        }

        private void CreateTables()
        {
            const string CreateTable = @"
                        CREATE TABLE IF NOT EXISTS {0} (
                                 Path TEXT PRIMARY KEY,
                                 OpenedAt INTEGER  NOT NULL
                        );
                    ";
            string createPathIndex = string.Format("CREATE INDEX IF NOT EXISTS IX_Path ON {0} (Path)", tableName);
            this.connection.ExecuteNonQuery(string.Format(CreateTable, tableName), createPathIndex);
        }

        public void Add(string item)
        {
            var result = this.connection.ExecuteScalar<long>(string.Format(@"SELECT count(1) FROM {0} WHERE Path = @Path", tableName), AddItem(item));

            Action<string> query = commandText => this.connection.ExecuteNonQuery(commandText, delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Path", item);
                DatabaseConnection.AddParameter(command, "@OpenedAt", DateTime.Now.Ticks);
            });

            if (result > 0)
            {
                query(string.Format("Update {0} SET OpenedAt = @OpenedAt WHERE Path = @Path", tableName));
                return;
            }

            query(string.Format(@"INSERT INTO {0}(Path, OpenedAt) VALUES (@Path, @OpenedAt)", tableName));
            
            result = this.connection.ExecuteScalar<long>(string.Format("SELECT count(1) FROM {0}", tableName));

            if (result <= this.maxItems)
            {
                return;
            }
            const string DeleteTemplate =
                @"DELETE FROM {1} 
                    WHERE OpenedAt IN (
                        SELECT OpenedAt FROM {1} ORDER BY OpenedAt ASC LIMIT {0}
                )";
            var cmdDelete = string.Format(DeleteTemplate, result - this.maxItems, tableName);
            this.connection.ExecuteNonQuery(cmdDelete);
        }

        private static Action<IDbCommand> AddItem(string file)
        {
            return cmd => DatabaseConnection.AddParameter(cmd, "@Path", file);
        }

        public void Remove(params string[] items)
        {
            string cmd = string.Format(@"DELETE FROM {0} WHERE Path = @Path", tableName);

            this.connection.BeginTran();
            foreach (var file in items)
            {
                this.connection.ExecuteNonQuery(cmd, Remove(file));
            }
            this.connection.CommitTran();
        }

        private static Action<IDbCommand> Remove(string item)
        {
            return command => DatabaseConnection.AddParameter(command, "@Path", item);
        }

        public IEnumerable<string> ReadItems()
        {
            var result = new List<string>(this.maxItems);
            this.connection.ExecuteReader(reader => result.Add(reader[0] as string), string.Format("SELECT Path FROM {0} ORDER BY OpenedAt DESC", tableName));
            return result;
        }
        
        public string ReadLastUsedItem()
        {
            var result = string.Empty;
            Action<IDataReader> onRead = delegate(IDataReader reader)
            {
                result = reader[0] as string;
            };
            this.connection.ExecuteReader(onRead, string.Format("SELECT Path FROM {0} ORDER BY OpenedAt DESC LIMIT 1", tableName));
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