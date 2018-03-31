// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 17.09.2013
// © 2012-2018 Alexander Egorov

using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using logviewer.logic.support;

namespace logviewer.logic.storage
{
    internal sealed class LocalDbConnection : IDatabaseConnection
    {
        private SQLiteConnection connection;

        private SQLiteTransaction transaction;

        private readonly SynchronizationContext creationContext;

        private bool disposed;

        internal LocalDbConnection(string databaseFilePath)
        {
            if (!File.Exists(databaseFilePath) || new FileInfo(databaseFilePath).Length == 0)
            {
                if (!databaseFilePath.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
                {
                    SQLiteConnection.CreateFile(databaseFilePath);
                }

                this.IsEmpty = true;
            }

            this.ConnectionString = databaseFilePath;
            var conString = new SQLiteConnectionStringBuilder { DataSource = databaseFilePath };
            this.creationContext = new SynchronizationContext();
            this.connection = new SQLiteConnection(conString.ToString());
            this.connection.Open();
        }

        ~LocalDbConnection() => this.DisposeInternal();

        public bool IsEmpty { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed",
                MessageId = "connection")]
        public void Dispose()
        {
            this.DisposeInternal();
            GC.SuppressFinalize(this);
        }

        private void DisposeInternal()
        {
            if (this.disposed)
            {
                return;
            }

            SafeRunner.Run(() => this.transaction?.Dispose());
            SafeRunner.Run(() => this.connection?.Close());
            SafeRunner.Run(() => this.connection?.Dispose());
            this.transaction = null;
            this.connection = null;
            this.disposed = true;
        }

        public void BeginTran() => this.ExecuteInCreationContext(o => this.transaction = this.connection.BeginTransaction());

        public void CommitTran() => this.ExecuteInCreationContext(o => this.transaction.Commit());

        public void RollbackTran() => this.ExecuteInCreationContext(o => this.transaction.Rollback());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void RunSqlQuery(Action<IDbCommand> action, params string[] commands)
        {
            void Method(object state)
            {
                try
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < commands.Length; i++)
                    {
                        this.RunCommand(action, commands[i]);
                    }
                }
                catch (SQLiteException e) when (HandleSqlException(e))
                {
                    Log.Instance.Debug(e);
                }
                catch (ObjectDisposedException e)
                {
                    Log.Instance.Debug(e);
                }
            }

            this.ExecuteInCreationContext(Method);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void RunSqlQuery(Action<IDbCommand> action, string command)
        {
            void Method(object state)
            {
                try
                {
                    this.RunCommand(action, command);
                }
                catch (SQLiteException e) when (HandleSqlException(e))
                {
                    Log.Instance.Debug(e);
                }
                catch (ObjectDisposedException e)
                {
                    Log.Instance.Debug(e);
                }
            }

            this.ExecuteInCreationContext(Method);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RunCommand(Action<IDbCommand> action, string command)
        {
            if (this.disposed)
            {
                return;
            }

            using (var cmd = new SQLiteCommand(this.connection))
            {
                cmd.CommandText = command;
                action(cmd);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HandleSqlException(SQLiteException e)
        {
            switch (e.ResultCode)
            {
                case SQLiteErrorCode.Abort:
                    return true;
                case SQLiteErrorCode.Misuse:
                    return true;
                default:
                    return false;
            }
        }

        public void ExecuteReader(string query, Action<IDataReader> onRead, Action<IDbCommand> beforeRead = null,
                                  Func<bool> notCancelled = null)
        {
            void Action(IDbCommand command)
            {
                beforeRead?.Invoke(command);
                var rdr = command.ExecuteReader();
                using (rdr)
                {
                    while (rdr.Read() && (notCancelled == null || notCancelled()))
                    {
                        onRead(rdr);
                    }
                }
            }

            this.RunSqlQuery(Action, query);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ExecuteInCreationContext(SendOrPostCallback method) => this.creationContext.Send(method, null);

        public T ExecuteScalar<T>(string query, Action<IDbCommand> actionBeforeExecute = null)
        {
            var result = default(T);

            void Action(IDbCommand cmd)
            {
                actionBeforeExecute?.Invoke(cmd);
                var r = cmd.ExecuteScalar();
                if (r != DBNull.Value)
                {
                    result = (T)r;
                }
            }

            this.RunSqlQuery(Action, query);
            return result;
        }

        public void ExecuteNonQuery(string query, Action<IDbCommand> actionBeforeExecute)
        {
            void Action(IDbCommand command)
            {
                try
                {
                    actionBeforeExecute?.Invoke(command);
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Log.Instance.Debug(e);
                }
            }

            this.RunSqlQuery(Action, query);
        }

        public void ExecuteNonQuery(params string[] queries) => this.RunSqlQuery(command => command.ExecuteNonQuery(), queries);

        public void ExecuteNonQuery(string query) => this.RunSqlQuery(command => command.ExecuteNonQuery(), query);

        public string ConnectionString { get; }
    }
}
