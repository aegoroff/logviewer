// Created by: egr
// Created at: 17.09.2013
// © 2012-2013 Alexander Egorov

using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace logviewer.core
{
    internal sealed class DatabaseConnection : IDisposable
    {
        private readonly SQLiteConnection connection;
        private readonly TaskScheduler executionContext;
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
            this.executionContext = TaskScheduler.Current;
            this.connection = new SQLiteConnection(conString.ToString());
            this.connection.Open();
        }

        internal bool IsEmpty { get; private set; }
        internal string DatabasePath { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed",
            MessageId = "connection")]
        public void Dispose()
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

        internal void BeginTran()
        {
            this.ExecuteInCreationContext(() => this.transaction = this.connection.BeginTransaction());
        }

        internal void CommitTran()
        {
            this.ExecuteInCreationContext(() => this.transaction.Commit());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        internal void RunSqlQuery(Action<IDbCommand> action, params string[] commands)
        {
            this.ExecuteInCreationContext(delegate
            {
                foreach (var command1 in commands)
                {
                    using (var sqLiteCommand1 = this.connection.CreateCommand())
                    {
                        sqLiteCommand1.CommandText = command1;
                        action(sqLiteCommand1);
                    }
                }
            });
        }

        private void ExecuteInCreationContext(Action method)
        {
            Task.Factory.StartNew(method, CancellationToken.None, TaskCreationOptions.None, this.executionContext).Wait();
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

        internal T ExecuteScalar<T>(string query, Action<IDbCommand> actionBeforeExecute = null)
        {
            var result = default(T);
            this.RunSqlQuery(delegate(IDbCommand cmd)
            {
                if (actionBeforeExecute != null)
                {
                    actionBeforeExecute(cmd);
                }
                result = (T)cmd.ExecuteScalar();
            }, query);
            return result;
        }

        internal void ExecuteNonQuery(string query, Action<IDbCommand> actionBeforeExecute = null)
        {
            this.RunSqlQuery(delegate(IDbCommand command)
            {
                try
                {
                    if (actionBeforeExecute != null)
                    {
                        actionBeforeExecute(command);
                    }
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Log.Instance.Debug(e);
                }
            }, query);
        }

        internal void ExecuteNonQuery(params string[] queries)
        {
            this.RunSqlQuery(command => command.ExecuteNonQuery(), queries);
        }
    }
}