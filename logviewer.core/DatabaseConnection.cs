// Created by: egr
// Created at: 17.09.2013
// © 2012-2013 Alexander Egorov

using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace logviewer.core
{
    internal sealed class DatabaseConnection : IDisposable
    {
        private readonly SQLiteConnection connection;
        private SQLiteTransaction transaction;

        internal DatabaseConnection(string databaseFilePath)
        {
            this.DatabasePath = databaseFilePath;
            if (!File.Exists(this.DatabasePath) || new FileInfo(this.DatabasePath).Length == 0)
            {
                SQLiteConnection.CreateFile(this.DatabasePath);
                this.IsEmpty = true;
            }
            var conString = new SQLiteConnectionStringBuilder { DataSource = this.DatabasePath };
            this.connection = new SQLiteConnection(conString.ToString());
            this.connection.Open();
        }

        internal bool IsEmpty { get; private set; }
        internal string DatabasePath { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
        }

        internal void BeginTran()
        {
            this.transaction = this.connection.BeginTransaction();
        }

        internal void CommitTran()
        {
            this.transaction.Commit();
        }

        internal void RunSqlQuery(Action<IDbCommand> action, params string[] commands)
        {
            try
            {
                foreach (var command in commands)
                {
                    using (var sqLiteCommand = this.connection.CreateCommand())
                    {
                        sqLiteCommand.CommandText = command;
                        action(sqLiteCommand);
                    }
                }
            }
            catch (ObjectDisposedException e)
            {
                Log.Instance.Debug(e);
            }
        }

        internal static void AddParameter(IDbCommand cmd, string name, object value)
        {
            var parameter = SQLiteFactory.Instance.CreateParameter();
            if (parameter == null)
            {
                return;
            }
            parameter.ParameterName = name;
            parameter.Value = value;
            cmd.Parameters.Add(parameter);
        }

        internal T ExecuteScalar<T>(string command, Action<IDbCommand> addParameters = null)
        {
            var result = default(T);
            this.RunSqlQuery(delegate(IDbCommand cmd)
            {
                if (addParameters != null)
                {
                    addParameters(cmd);
                }
                result = (T)cmd.ExecuteScalar();
            }, command);
            return result;
        }

        internal void ExecuteNonQuery(params string[] queries)
        {
            this.RunSqlQuery(command => command.ExecuteNonQuery(), queries);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.transaction != null)
                {
                    this.transaction.Dispose();
                }
                if (this.connection != null)
                {
                    SafeRunner.Run(this.connection.Close);
                    SafeRunner.Run(this.connection.Dispose);
                }
            }
        }
    }
}