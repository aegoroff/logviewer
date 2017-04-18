// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 18.04.2017
// © 2012-2017 Alexander Egorov

using System;
using System.Data;

namespace logviewer.logic.storage
{
    public interface IDatabaseConnection : IDisposable
    {
        bool IsEmpty { get; }

        void BeginTran();

        void CommitTran();

        void RollbackTran();

        void RunSqlQuery(Action<IDbCommand> action, params string[] commands);

        void RunSqlQuery(Action<IDbCommand> action, string command);

        void ExecuteReader(string query, Action<IDataReader> onRead, Action<IDbCommand> beforeRead = null,
                           Func<bool> notCancelled = null);

        T ExecuteScalar<T>(string query, Action<IDbCommand> actionBeforeExecute = null);

        void ExecuteNonQuery(string query, Action<IDbCommand> actionBeforeExecute);

        void ExecuteNonQuery(params string[] queries);

        void ExecuteNonQuery(string query);
    }
}
