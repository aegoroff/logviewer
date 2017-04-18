// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 18.04.2017
// © 2012-2017 Alexander Egorov

using System;
using System.Data;

namespace logviewer.logic.storage
{
    internal class LocalDbQueryProvider : IQueryProvider
    {
        private readonly string settingsDatabaseFilePath;

        internal LocalDbQueryProvider(string settingsDatabaseFilePath)
        {
            this.settingsDatabaseFilePath = settingsDatabaseFilePath;
        }

        public T ExecuteScalar<T>(string query, Action<IDbCommand> actionBeforeExecute = null)
        {
            var result = default(T);

            void Action(IDatabaseConnection connection)
            {
                result = connection.ExecuteScalar<T>(query, actionBeforeExecute);
            }

            this.ExecuteQuery(Action);
            return result;
        }

        public void ExecuteNonQuery(string query, Action<IDbCommand> actionBeforeExecute = null) =>
                this.ExecuteQuery(connection => connection.ExecuteNonQuery(query, actionBeforeExecute));

        public void ExecuteQuery(Action<IDatabaseConnection> action)
        {
            IDatabaseConnection connection = new LocalDbConnection(this.settingsDatabaseFilePath);
            using (connection)
            {
                action(connection);
            }
        }
    }
}
