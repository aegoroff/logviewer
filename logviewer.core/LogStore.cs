// Created by: egr
// Created at: 14.09.2013
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.Devices;

namespace logviewer.core
{
    public sealed class LogStore : IDisposable
    {
        private readonly LogMessageParseOptions parseOptions;

        #region Constants and Fields

        private const string CreateColumnTemplate = @"{0} INTEGER NOT NULL";
        private const string CreateIndexTemplate = @"CREATE INDEX IF NOT EXISTS IX_{0} ON Log ({0})";
        private const string DropIndexTemplate = @"DROP INDEX IF EXISTS IX_{0}";
        private const int PageSize = 1024;
        private readonly DatabaseConnection connection;

        private static readonly IDictionary<LogMessageParseOptions, string> additionalColumns = new Dictionary<LogMessageParseOptions, string>
        {
            {LogMessageParseOptions.LogLevel, "Level"},
            {LogMessageParseOptions.DateTime, "Datetime"}
        };

        #endregion

        #region Constructors and Destructors

        public LogStore(long dbSize = 0L, string databaseFilePath = null, LogMessageParseOptions parseOptions = LogMessageParseOptions.LogLevel)
        {
            this.parseOptions = parseOptions;
            this.DatabasePath = databaseFilePath ?? Path.GetTempFileName();
            this.connection = new DatabaseConnection(this.DatabasePath);
            this.CreateTables(dbSize);
        }

        private IEnumerable<string> CreateAdditionalColumns()
        {
            return this.ReadAdditionalColumns().Select(column => string.Format(CreateColumnTemplate, column));
        }

        private IEnumerable<string> CreateAdditionalIndexes()
        {
            return this.ReadAdditionalColumns().Select(column => string.Format(CreateIndexTemplate, column));
        }
        
        private IEnumerable<string> DropAdditionalIndexes()
        {
            return this.ReadAdditionalColumns().Select(column => string.Format(DropIndexTemplate, column));
        }

        private IEnumerable<string> ReadAdditionalColumns()
        {
            return from column in additionalColumns where this.parseOptions.HasFlag(column.Key) select column.Value;
        }

        public string DatabasePath { get; private set; }

        private void CreateTables(long dbSize)
        {
            if (!this.connection.IsEmpty)
            {
                return;
            }
            const string createTableTemplate = @"
                        CREATE TABLE IF NOT EXISTS Log (
                                 Ix INTEGER PRIMARY KEY,
                                 Header TEXT  NOT NULL,
                                 Body  TEXT
                                 {0}
                        );
                    ";

            const string syncOff = @"PRAGMA synchronous = OFF;";
            const string journal = @"PRAGMA journal_mode = OFF;";
            const string cacheTemplate = @"PRAGMA cache_size = {0};";
            const string temp = @"PRAGMA temp_store = MEMORY;";
            const string encode = @"PRAGMA encoding = 'UTF-8';";
            const string mmapTemplate = @"PRAGMA mmap_size={0};";

            var freePages = new ComputerInfo().AvailablePhysicalMemory / PageSize;
            var sqliteAvailablePages = (int)(freePages * 0.2);

            var pages = Math.Min(sqliteAvailablePages, dbSize / PageSize + 1);

            var colums = string.Join(",", this.CreateAdditionalColumns());
            var additionalCreate = string.IsNullOrWhiteSpace(colums) ? string.Empty : "," + colums;

            var mmap = string.Format(mmapTemplate, dbSize);
            var cache = string.Format(cacheTemplate, pages);
            var createTable = string.Format(createTableTemplate, additionalCreate);
            this.connection.ExecuteNonQuery(syncOff, journal, cache, temp, encode, mmap, createTable);
            this.Index();
        }

        #endregion

        #region Public Methods and Operators

        public void Dispose()
        {
            this.Dispose(true);
        }

        public void StartAddMessages()
        {
            this.NoIndex();
            this.connection.BeginTran();
        }

        public void FinishAddMessages()
        {
            this.connection.CommitTran();
            this.Index();
        }

        public void Index()
        {
            this.connection.ExecuteNonQuery(this.CreateAdditionalIndexes().ToArray());
        }

        public void NoIndex()
        {
            this.connection.ExecuteNonQuery(this.DropAdditionalIndexes().ToArray());
        }

        public void AddMessage(LogMessage message)
        {
            // ugly but very fast
            var cmd = @"INSERT INTO Log(Ix, Header, Body, Level) VALUES (" + message.Ix + ", @Header, @Body, " + (int)message.Level + ")";
            Action<IDbCommand> action = delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Header", message.Header);
                DatabaseConnection.AddParameter(command, "@Body", message.Body);
            };
            this.connection.ExecuteNonQuery(cmd, action);
        }

        public void ReadMessages(
            int limit,
            Action<LogMessage> onReadMessage,
            Func<bool> notCancelled,
            long offset = 0,
            bool reverse = true,
            LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true)
        {
            var order = reverse ? "DESC" : "ASC";

            var where = Where(min, max, filter, useRegexp);
            var query = string.Format("SELECT Header, Body, Level FROM Log {3} ORDER BY Ix {0} LIMIT {1} OFFSET {2}",
                order, limit, offset,
                where);
            Action<IDbCommand> beforeRead = command => AddParameters(command, min, max, filter, useRegexp);
            Action<IDataReader> onRead = delegate(IDataReader rdr)
            {
                var msg = new LogMessage(rdr[0] as string, rdr[1] as string, (LogLevel) ((long) rdr[2]));
                onReadMessage(msg);
            };
            this.connection.ExecuteReader(onRead, query, beforeRead, notCancelled);
        }

        public long CountMessages(
            LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true)
        {
            var where = Where(min, max, filter, useRegexp);
            var query = string.Format(@"SELECT count(1) FROM Log {0}", where);
            Action<IDbCommand> addParameters = cmd => AddParameters(cmd, min, max, filter, useRegexp);
            var result = this.connection.ExecuteScalar<long>(query, addParameters);
            return result;
        }

        private static string Where(LogLevel min, LogLevel max, string filter, bool useRegexp)
        {
            var clauses = new[]
            {
                LevelClause(min, max),
                FilterClause(filter, useRegexp)
            };
            var notEmpty = clauses.Where(clause => !string.IsNullOrWhiteSpace(clause)).ToArray();
            if (!notEmpty.Any())
            {
                return string.Empty;
            }
            return "WHERE " + string.Join(" AND ", notEmpty);
        }

        private static string LevelClause(LogLevel min, LogLevel max)
        {
            var clause = new List<string>();
            if (min != LogLevel.Trace)
            {
                clause.Add("Level >= @Min");
            }
            if (max != LogLevel.Fatal)
            {
                clause.Add("Level <= @Max");
            }
            return string.Join(" AND ", clause);
        }

        private static string FilterClause(string filter, bool useRegexp)
        {
            var comparer = useRegexp ? "REGEXP" : "LIKE";
            var func = string.Format("(Header || Body) {0} @Filter", comparer);
            return string.IsNullOrWhiteSpace(filter) ? string.Empty : func;
        }

        private static void AddParameters(IDbCommand command, LogLevel min, LogLevel max, string filter, bool useRegexp)
        {
            if (min != LogLevel.Trace)
            {
                DatabaseConnection.AddParameter(command, "@Min", (int)min);
            }
            if (max != LogLevel.Fatal)
            {
                DatabaseConnection.AddParameter(command, "@Max", (int)max);
            }
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var f = useRegexp ? filter : string.Format("%{0}%", filter.Trim('%'));
                DatabaseConnection.AddParameter(command, "@Filter", f);
            }
        }

        #endregion

        #region Methods

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.connection != null)
                {
                    this.connection.Dispose();
                }
                if (File.Exists(this.DatabasePath))
                {
                    File.Delete(this.DatabasePath);
                }
            }
        }

        #endregion
    }
}