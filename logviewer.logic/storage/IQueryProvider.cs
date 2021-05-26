// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 18.04.2017
// © 2012-2018 Alexander Egorov

using System;
using System.Data;

namespace logviewer.logic.storage
{
    public interface IQueryProvider
    {
        T ExecuteScalar<T>(string query, Action<IDbCommand> actionBeforeExecute = null);

        void ExecuteNonQuery(string query, Action<IDbCommand> actionBeforeExecute = null);

        void ExecuteQuery(Action<IDatabaseConnection> action);
    }
}