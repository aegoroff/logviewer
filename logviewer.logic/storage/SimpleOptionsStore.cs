// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 03.12.2015
// © 2012-2018 Alexander Egorov

using System;
using System.Data;

namespace logviewer.logic.storage
{
    internal sealed class SimpleOptionsStore : ISimpleOptionsStore
    {
        private const string StringOptionsTable = @"StringOptions";

        private const string BooleanOptionsTable = @"BooleanOptions";

        private const string IntegerOptionsTable = @"IntegerOptions";

        private const string OptionParameter = @"@Option";

        private const string ValueParameter = @"@Value";

        private readonly IQueryProvider queryProvider;

        internal SimpleOptionsStore(IQueryProvider queryProvider)
        {
            this.queryProvider = queryProvider;

            const string optionsTableTemplate = @"
                        CREATE TABLE IF NOT EXISTS {0} (
                                 Option TEXT PRIMARY KEY,
                                 Value {1}
                        );
                    ";

            var stringOptions = string.Format(optionsTableTemplate, StringOptionsTable, @"TEXT");
            var integerOptions = string.Format(optionsTableTemplate, IntegerOptionsTable, @"INTEGER");
            var booleanOptions = string.Format(optionsTableTemplate, BooleanOptionsTable, @"BOOLEAN");

            this.queryProvider.ExecuteQuery(connection => connection.RunSqlQuery(command => command.ExecuteNonQuery(), stringOptions,
                                                                                 integerOptions, booleanOptions));
        }

        public string ReadStringOption(string option, string defaultValue = null) => this.ReadOption(StringOptionsTable, option,
                                                                                                     defaultValue);

        public bool ReadBooleanOption(string option, bool defaultValue = false) => this.ReadOption(BooleanOptionsTable, option,
                                                                                                   defaultValue);

        public int ReadIntegerOption(string option, int defaultValue = 0) => (int)this.ReadOption<long>(IntegerOptionsTable, option,
                                                                                                        defaultValue);

        public void UpdateStringOption(string option, string value) => this.UpdateOption(StringOptionsTable, option, value);

        public void UpdateBooleanOption(string option, bool value) => this.UpdateOption(BooleanOptionsTable, option, value);

        public void UpdateIntegerOption(string option, int value) => this.UpdateOption(IntegerOptionsTable, option, value);

        private T ReadOption<T>(string table, string option, T defaultValue = default(T))
        {
            var cmd = $"SELECT Value FROM {{0}} WHERE Option = {OptionParameter}";
            var result = default(T);
            var read = false;

            void OnRead(IDataReader rdr)
            {
                if (rdr[0] is DBNull)
                {
                    return;
                }

                result = (T)rdr[0];
                read = true;
            }

            void BeforeRead(IDbCommand command) => command.AddParameter(OptionParameter, option);

            void Action(IDatabaseConnection connection) => connection.ExecuteReader(string.Format(cmd, table), OnRead, BeforeRead);

            this.queryProvider.ExecuteQuery(Action);

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
            var exist = this.queryProvider.ExecuteScalar<long>(
                                                               $@"SELECT count(1) FROM {table} WHERE Option = {OptionParameter}",
                                                               command => command.AddParameter(OptionParameter, option))
                        > 0;

            void Action(IDbCommand command)
            {
                command.AddParameter(OptionParameter, option);
                command.AddParameter(ValueParameter, value);
            }

            var format = exist
                                 ? $"UPDATE {{0}} SET Value = {ValueParameter} WHERE Option = {OptionParameter}"
                                 : $"INSERT INTO {{0}} (Option, Value) VALUES ({OptionParameter}, {ValueParameter})";
            var updateQuery = string.Format(format, table);
            this.queryProvider.ExecuteNonQuery(updateQuery, Action);
        }
    }
}
