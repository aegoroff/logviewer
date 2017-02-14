// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 03.12.2015
// © 2012-2016 Alexander Egorov

using System;
using System.Data;

namespace logviewer.logic.storage
{
    internal sealed class OptionsProvider : IOptionsProvider
    {
        private const string StringOptionsTable = @"StringOptions";
        private const string BooleanOptionsTable = @"BooleanOptions";
        private const string IntegerOptionsTable = @"IntegerOptions";
        private const string OptionParameter = @"@Option";
        private const string ValueParameter = @"@Value";
        private readonly string settingsDatabaseFilePath;

        public OptionsProvider(string settingsDatabaseFilePath)
        {
            this.settingsDatabaseFilePath = settingsDatabaseFilePath;
        }

        public string ReadStringOption(string option, string defaultValue = null)
        {
            return this.ReadOption(StringOptionsTable, option, defaultValue);
        }

        public bool ReadBooleanOption(string option, bool defaultValue = false)
        {
            return this.ReadOption(BooleanOptionsTable, option, defaultValue);
        }

        public int ReadIntegerOption(string option, int defaultValue = 0)
        {
            return (int) this.ReadOption<long>(IntegerOptionsTable, option, defaultValue);
        }

        public void UpdateStringOption(string option, string value)
        {
            this.UpdateOption(StringOptionsTable, option, value);
        }

        public void UpdateBooleanOption(string option, bool value)
        {
            this.UpdateOption(BooleanOptionsTable, option, value);
        }

        public void UpdateIntegerOption(string option, int value)
        {
            this.UpdateOption(IntegerOptionsTable, option, value);
        }

        internal T ExecuteScalar<T>(string query, Action<IDbCommand> actionBeforeExecute = null)
        {
            var result = default(T);
            Action<DatabaseConnection> action =
                delegate(DatabaseConnection connection) { result = connection.ExecuteScalar<T>(query, actionBeforeExecute); };
            this.ExecuteQuery(action);
            return result;
        }

        internal void ExecuteNonQuery(string query, Action<IDbCommand> actionBeforeExecute = null)
        {
            this.ExecuteQuery(connection => connection.ExecuteNonQuery(query, actionBeforeExecute));
        }

        internal void ExecuteQuery(Action<DatabaseConnection> action)
        {
            var connection = new DatabaseConnection(this.settingsDatabaseFilePath);
            using (connection)
            {
                action(connection);
            }
        }

        private T ReadOption<T>(string table, string option, T defaultValue = default(T))
        {
            string cmd = $"SELECT Value FROM {{0}} WHERE Option = {OptionParameter}";
            var result = default(T);
            var read = false;

            Action<IDataReader> onRead = delegate(IDataReader rdr)
            {
                if (rdr[0] is DBNull)
                {
                    return;
                }
                result = (T) rdr[0];
                read = true;
            };

            Action<IDbCommand> beforeRead = command => command.AddParameter(OptionParameter, option);
            Action<DatabaseConnection> action = connection => connection.ExecuteReader(string.Format(cmd, table), onRead, beforeRead);
            this.ExecuteQuery(action);


            if (read)
            {
                return result;
            }
            this.UpdateOption(table, option, defaultValue);
            result = defaultValue;
            return result;
        }

        private void UpdateOption<T>(string table, string option, T value)
        {
            var exist = this.ExecuteScalar<long>(
                $@"SELECT count(1) FROM {table} WHERE Option = {OptionParameter}",
                command => command.AddParameter(OptionParameter, option)) > 0;

            Action<IDbCommand> action = delegate(IDbCommand command)
            {
                command.AddParameter(OptionParameter, option);
                command.AddParameter(ValueParameter, value);
            };

            var format = exist 
                ? $"UPDATE {{0}} SET Value = {ValueParameter} WHERE Option = {OptionParameter}"
                : $"INSERT INTO {{0}} (Option, Value) VALUES ({OptionParameter}, {ValueParameter})";
            var updateQuery = string.Format(format, table);
            this.ExecuteNonQuery(updateQuery, action);
        }
    }
}