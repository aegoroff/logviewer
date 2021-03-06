﻿// Created by: egr
// Created at: 14.09.2013
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using logviewer.engine;
using Microsoft.VisualBasic.Devices;

namespace logviewer.core
{
    public sealed class LogStore : IDisposable
    {
        #region Constants and Fields

        private const string CreateColumnTemplate = @"{0} INTEGER NOT NULL";
        private const string CreateIndexTemplate = @"CREATE INDEX IF NOT EXISTS IX_{0} ON Log ({0})";
        private const string DropIndexTemplate = @"DROP INDEX IF EXISTS IX_{0}";
        private const int PageSize = 1024;
        private readonly DatabaseConnection connection;
        private string additionalColumnsString;
        private string[] additionalColumns;
        private string additionalParametersString;
        private readonly bool hasLogLevelProperty;
        private readonly string logLevelProperty;
        private readonly bool hasDateTimeProperty;
        private readonly string dateTimeProperty;
        private readonly IDictionary<SemanticProperty, ISet<GrokRule>> rules;
        private IDictionary<string, PropertyType> propertyTypesCache;
        private readonly RulesBuilder builder;

        #endregion

        #region Constructors and Destructors

        public LogStore(long dbSize = 0L, string databaseFilePath = null, ICollection<Semantic> schema = null)
        {
            this.builder = new RulesBuilder(schema);
            this.rules = this.builder.Rules;
            this.hasLogLevelProperty = schema.HasProperty(ParserType.LogLevel);
            this.logLevelProperty = schema.PropertyNameOf(ParserType.LogLevel);
            this.hasDateTimeProperty = schema.HasProperty(ParserType.Datetime);
            this.dateTimeProperty = schema.PropertyNameOf(ParserType.Datetime);

            this.DatabasePath = databaseFilePath ?? Path.GetTempFileName();
            this.connection = new DatabaseConnection(this.DatabasePath);
            this.CreateTables(dbSize);
        }

        private IEnumerable<string> CreateAdditionalColumns()
        {
            return this.DdlHelper(CreateColumnTemplate);
        }

        private IEnumerable<string> CreateAdditionalIndexes()
        {
            return this.DdlHelper(CreateIndexTemplate);
        }
        
        private IEnumerable<string> DropAdditionalIndexes()
        {
            return this.DdlHelper(DropIndexTemplate);
        }
        
        private IEnumerable<string> DdlHelper(string template)
        {
            return this.ReadAdditionalColumns(column => string.Format(template, column));
        }

        private IEnumerable<string> ReadAdditionalColumns(Func<string, string> selector)
        {
            return this.rules.Keys.Select(r => selector(r.Name));
        }
        
        
        private string CreateAdditionalColumnsList(string prefix = null)
        {
            var colums = string.Join(",", this.ReadAdditionalColumns(s => prefix + s));
            return string.IsNullOrWhiteSpace(colums) ? string.Empty : "," + colums;
        }


        public string DatabasePath { get; private set; }

        public bool HasLogLevelProperty
        {
            get { return this.hasLogLevelProperty; }
        }

        public string LogLevelProperty
        {
            get { return this.logLevelProperty; }
        }

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
            this.additionalColumns = this.ReadAdditionalColumns(c => c).ToArray();
            this.additionalColumnsString = this.CreateAdditionalColumnsList();
            this.additionalParametersString = this.CreateAdditionalColumnsList("@");
            this.propertyTypesCache = new Dictionary<string, PropertyType>();
            foreach (var column in this.additionalColumns)
            {
                this.propertyTypesCache.Add(column, this.DefinePropertyType(column));
            }
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
            message.Cache(this.rules);
            // ugly but very fast
            var cmd = @"INSERT INTO Log(Ix, Header, Body" + this.additionalColumnsString + ") VALUES (" + message.Ix + ", @Header, @Body " + this.additionalParametersString + ")";
            Action<IDbCommand> action = delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Header", message.Header);
                DatabaseConnection.AddParameter(command, "@Body", message.Body);
// ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < this.additionalColumns.Length; i++)
                {
                    var column = this.additionalColumns[i];
                    if (this.propertyTypesCache[column] == PropertyType.Integer)
                    {
                        DatabaseConnection.AddParameter(command, "@" + column, message.IntegerProperty(column));
                    }
                    else
                    {
                        DatabaseConnection.AddParameter(command, "@" + column, message.StringProperty(column));
                    }
                }
            };
            this.connection.ExecuteNonQuery(cmd, action);
        }

        public void ReadMessages(
            int limit,
            Action<LogMessage> onReadMessage,
            Func<bool> notCancelled,
            DateTime start,
            DateTime finish,
            long offset = 0,
            bool reverse = true,
            LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true)
        {
            var order = reverse ? "DESC" : "ASC";

            var where = this.Where(min, max, filter, useRegexp, start, finish);
            
            var query = string.Format("SELECT Header, Body {4} FROM Log {3} ORDER BY Ix {0} LIMIT {1} OFFSET {2}",
                order, limit, offset,
                where, this.additionalColumnsString);
            Action<IDbCommand> beforeRead = command => this.AddParameters(command, min, max, filter, useRegexp, start, finish);
            Action<IDataReader> onRead = delegate(IDataReader rdr)
            {
                var msg = new LogMessage(rdr[0] as string, rdr[1] as string);
                foreach (var column in this.additionalColumns)
                {
                    if (this.propertyTypesCache[column] == PropertyType.Integer)
                    {
                        msg.UpdateIntegerProperty(column, (long)rdr[column]);
                    }
                    else
                    {
                        msg.UpdateStringProperty(column, rdr[column] as string);
                    }
                }

                onReadMessage(msg);
            };
            this.connection.ExecuteReader(onRead, query, beforeRead, notCancelled);
        }

        private PropertyType DefinePropertyType(string param)
        {
            var result = this.builder.DefineParserType(param);
            return result == ParserType.String ? PropertyType.String : PropertyType.Integer;
        }

        public long CountMessages(
            LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true,
            bool excludeNoLevel = false)
        {
            return this.CountMessages(DateTime.MinValue, DateTime.MaxValue, min, max, filter, useRegexp, excludeNoLevel);
        }
        
        public long CountMessages(
            DateTime start, 
            DateTime finish,
            LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true,
            bool excludeNoLevel = false)
        {
            if (excludeNoLevel && !this.hasLogLevelProperty)
            {
                return 0;
            }

            var where = this.Where(min, max, filter, useRegexp, start, finish);
            var query = string.Format(@"SELECT count(1) FROM Log {0}", where);
            Action<IDbCommand> addParameters = cmd => this.AddParameters(cmd, min, max, filter, useRegexp, start, finish);
            var result = this.connection.ExecuteScalar<long>(query, addParameters);
            return result;
        }
        
        public DateTime SelectDateUsingFunc(string func, LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true)
        {
            if (!this.hasDateTimeProperty)
            {
                return DateTime.MinValue;
            }
            
            var where = this.Where(min, max, filter, useRegexp, DateTime.MinValue, DateTime.MaxValue);
            var query = string.Format(@"SELECT {2}({0}) FROM Log {1}", this.dateTimeProperty, where, func);
            Action<IDbCommand> addParameters = cmd => this.AddParameters(cmd, min, max, filter, useRegexp, DateTime.MinValue, DateTime.MaxValue);
            var result = this.connection.ExecuteScalar<long>(query, addParameters);
            return DateTime.FromFileTime(result);
        }

        private string Where(LogLevel min, LogLevel max, string filter, bool useRegexp, DateTime start, DateTime finish, bool excludeNoLevel = false)
        {
            var clauses = new[]
            {
                this.LevelClause(min, max), 
                this.DateClause(start, finish), 
                FilterClause(filter, useRegexp), 
                this.ExcludeNoLevelClause(excludeNoLevel)
            };
            var notEmpty = clauses.Where(clause => !string.IsNullOrWhiteSpace(clause)).ToArray();
            if (!notEmpty.Any())
            {
                return string.Empty;
            }
            return "WHERE " + string.Join(" AND ", notEmpty);
        }

        private string LevelClause(LogLevel min, LogLevel max)
        {
            if (!this.hasLogLevelProperty)
            {
                return string.Empty;
            }
            var clause = new List<string>();
            if (min != LogLevel.Trace)
            {
                clause.Add(this.logLevelProperty + " >= @Min");
            }
            if (max != LogLevel.Fatal)
            {
                clause.Add(this.logLevelProperty + " <= @Max");
            }
            return string.Join(" AND ", clause);
        }

        private string DateClause(DateTime start, DateTime finish)
        {
            if (!this.hasDateTimeProperty)
            {
                return string.Empty;
            }
            var clause = new List<string>();
            if (start != DateTime.MinValue)
            {
                clause.Add(this.dateTimeProperty + " >= @Start");
            }
            if (finish != DateTime.MaxValue)
            {
                clause.Add(this.dateTimeProperty + " <= @Finish");
            }
            return string.Join(" AND ", clause);
        }

        private static string FilterClause(string filter, bool useRegexp)
        {
            var comparer = useRegexp ? "REGEXP" : "LIKE";
            var func = string.Format("(Header || Body) {0} @Filter", comparer);
            return string.IsNullOrWhiteSpace(filter) ? string.Empty : func;
        }

        private string ExcludeNoLevelClause(bool excludeNoLevel)
        {
            return excludeNoLevel && this.hasLogLevelProperty ? this.logLevelProperty + " > " + (int)LogLevel.None : string.Empty;
        }

        private void AddParameters(IDbCommand command, LogLevel min, LogLevel max, string filter, bool useRegexp, DateTime start, DateTime finish)
        {
            if (min != LogLevel.Trace && this.hasLogLevelProperty)
            {
                DatabaseConnection.AddParameter(command, "@Min", (int) min);
            }
            if (max != LogLevel.Fatal && this.hasLogLevelProperty)
            {
                DatabaseConnection.AddParameter(command, "@Max", (int) max);
            }
            if (start != DateTime.MinValue && this.hasDateTimeProperty)
            {
                DatabaseConnection.AddParameter(command, "@Start", start.ToFileTimeUtc());
            }
            if (finish != DateTime.MaxValue && this.hasDateTimeProperty)
            {
                DatabaseConnection.AddParameter(command, "@Finish", finish.ToFileTimeUtc());
            }
            if (!string.IsNullOrWhiteSpace(filter))
            {
                string f = useRegexp ? filter : string.Format("%{0}%", filter.Trim('%'));
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