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

        public RecentItemsStore(ISettingsProvider settings, string tableName, int maxItems = 0)
        {
            this.tableName = tableName;
            this.maxItems = maxItems == 0 ? settings.KeepLastNFiles : maxItems;
            this.connection = new DatabaseConnection(settings.FullPathToDatabase);
            this.CreateTables();
        }

        private void CreateTables()
        {
            const string CreateTable = @"
                        CREATE TABLE IF NOT EXISTS {0} (
                                 Item TEXT PRIMARY KEY,
                                 UsedAt INTEGER  NOT NULL
                        );
                    ";
            string createItemIndex = string.Format("CREATE INDEX IF NOT EXISTS IX_Item ON {0} (Item)", tableName);
            this.connection.ExecuteNonQuery(string.Format(CreateTable, tableName), createItemIndex);
        }

        public void Add(string item)
        {
            var result = this.connection.ExecuteScalar<long>(string.Format(@"SELECT count(1) FROM {0} WHERE Item = @Item", tableName), AddItem(item));

            Action<string> query = commandText => this.connection.ExecuteNonQuery(commandText, delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Item", item);
                DatabaseConnection.AddParameter(command, "@UsedAt", DateTime.Now.Ticks);
            });

            if (result > 0)
            {
                query(string.Format("Update {0} SET UsedAt = @UsedAt WHERE Item = @Item", tableName));
                return;
            }

            query(string.Format(@"INSERT INTO {0}(Item, UsedAt) VALUES (@Item, @UsedAt)", tableName));
            
            result = this.connection.ExecuteScalar<long>(string.Format("SELECT count(1) FROM {0}", tableName));

            if (result <= this.maxItems)
            {
                return;
            }
            const string DeleteTemplate =
                @"DELETE FROM {1} 
                    WHERE UsedAt IN (
                        SELECT UsedAt FROM {1} ORDER BY UsedAt ASC LIMIT {0}
                )";
            var cmdDelete = string.Format(DeleteTemplate, result - this.maxItems, tableName);
            this.connection.ExecuteNonQuery(cmdDelete);
        }

        private static Action<IDbCommand> AddItem(string file)
        {
            return cmd => DatabaseConnection.AddParameter(cmd, "@Item", file);
        }

        public void Remove(params string[] items)
        {
            string cmd = string.Format(@"DELETE FROM {0} WHERE Item = @Item", tableName);

            this.connection.BeginTran();
            foreach (var file in items)
            {
                this.connection.ExecuteNonQuery(cmd, Remove(file));
            }
            this.connection.CommitTran();
        }

        private static Action<IDbCommand> Remove(string item)
        {
            return command => DatabaseConnection.AddParameter(command, "@Item", item);
        }

        public IEnumerable<string> ReadItems()
        {
            var result = new List<string>(this.maxItems);
            this.connection.ExecuteReader(reader => result.Add(reader[0] as string), string.Format("SELECT Item FROM {0} ORDER BY UsedAt DESC", tableName));
            return result;
        }
        
        public string ReadLastUsedItem()
        {
            var result = string.Empty;
            Action<IDataReader> onRead = delegate(IDataReader reader)
            {
                result = reader[0] as string;
            };
            this.connection.ExecuteReader(onRead, string.Format("SELECT Item FROM {0} ORDER BY UsedAt DESC LIMIT 1", tableName));
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