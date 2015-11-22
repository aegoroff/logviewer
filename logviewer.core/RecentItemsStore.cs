// Created by: egr
// Created at: 17.09.2013
// © 2012-2015 Alexander Egorov

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
            string createTable = $@"CREATE TABLE IF NOT EXISTS {this.tableName} (
                                 Item TEXT PRIMARY KEY,
                                 UsedAt INTEGER  NOT NULL
                        );";
            string createItemIndex = $"CREATE INDEX IF NOT EXISTS IX_Item ON {this.tableName} (Item)";
            this.connection.ExecuteNonQuery(createTable, createItemIndex);
        }

        public void Add(string item)
        {
            var result = this.connection.ExecuteScalar<long>($@"SELECT count(1) FROM {this.tableName} WHERE Item = @Item", AddItem(item));

            Action<string> query = commandText => this.connection.ExecuteNonQuery(commandText, delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Item", item);
                DatabaseConnection.AddParameter(command, "@UsedAt", DateTime.Now.Ticks);
            });

            if (result > 0)
            {
                query($"Update {this.tableName} SET UsedAt = @UsedAt WHERE Item = @Item");
                return;
            }

            query($@"INSERT INTO {this.tableName}(Item, UsedAt) VALUES (@Item, @UsedAt)");
            
            result = this.connection.ExecuteScalar<long>($"SELECT count(1) FROM {this.tableName}");

            if (result <= this.maxItems)
            {
                return;
            }
            const string deleteTemplate =
                @"DELETE FROM {1} 
                    WHERE UsedAt IN (
                        SELECT UsedAt FROM {1} ORDER BY UsedAt ASC LIMIT {0}
                )";
            var cmdDelete = string.Format(deleteTemplate, result - this.maxItems, this.tableName);
            this.connection.ExecuteNonQuery(cmdDelete);
        }

        private static Action<IDbCommand> AddItem(string file)
        {
            return cmd => DatabaseConnection.AddParameter(cmd, "@Item", file);
        }

        public void Remove(params string[] items)
        {
            string cmd = $@"DELETE FROM {this.tableName} WHERE Item = @Item";

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
            this.connection.ExecuteReader(reader => result.Add(reader[0] as string),
                $"SELECT Item FROM {this.tableName} ORDER BY UsedAt DESC");
            return result;
        }
        
        public string ReadLastUsedItem()
        {
            var result = string.Empty;
            Action<IDataReader> onRead = delegate(IDataReader reader)
            {
                result = reader[0] as string;
            };
            this.connection.ExecuteReader(onRead, $"SELECT Item FROM {this.tableName} ORDER BY UsedAt DESC LIMIT 1");
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