// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 14.09.2013
// © 2012-2017 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using logviewer.engine;
using logviewer.logic.Annotations;
using logviewer.logic.support;
using Microsoft.VisualBasic.Devices;

namespace logviewer.logic.storage
{
    public sealed class LogStore : IDisposable, ILogStore
    {
        #region Constants and Fields

        private const string CreateColumnTemplate = @"{0} INTEGER NOT NULL";
        private const string CreateIndexTemplate = @"CREATE INDEX IF NOT EXISTS IX_{0} ON Log ({0})";
        private const string DropIndexTemplate = @"DROP INDEX IF EXISTS IX_{0}";
        private const int PageSize = 1024;
        private const int DelaySeconds = 10;
        private readonly DatabaseConnection connection;
        private string[] additionalColumns;
        private readonly bool hasLogLevelProperty;
        private readonly string logLevelProperty;
        private readonly bool hasDateTimeProperty;
        private readonly string dateTimeProperty;
        private readonly IDictionary<SemanticProperty, ISet<GrokRule>> rules;
        private Dictionary<string, PropertyType> propertyTypesCache;
        private readonly RulesBuilder builder;
        private string insertPrefix;
        private string insertSuffix;

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
            var colums = string.Join(",", this.ReadAdditionalColumns(s => prefix + s)); // Not L10N
            return string.IsNullOrWhiteSpace(colums) ? string.Empty : "," + colums; // Not L10N
        }

        public string DatabasePath { get; }

        public bool HasLogLevelProperty => this.hasLogLevelProperty;

        public string LogLevelProperty => this.logLevelProperty;

        private void CreateTables(long dbSize)
        {
            if (!this.connection.IsEmpty)
            {
                return;
            }

            const string syncOff = @"PRAGMA synchronous = OFF;";
            const string journal = @"PRAGMA journal_mode = OFF;";
            const string temp = @"PRAGMA temp_store = MEMORY;";
            const string encode = @"PRAGMA encoding = 'UTF-8';";

            var freePages = new ComputerInfo().AvailablePhysicalMemory / PageSize;
            var sqliteAvailablePages = (int)(freePages * 0.2);

            var pages = Math.Min(sqliteAvailablePages, dbSize / PageSize + 1);

            var colums = string.Join(",", this.CreateAdditionalColumns()); // Not L10N
            var additionalCreate = string.IsNullOrWhiteSpace(colums) ? string.Empty : "," + colums; // Not L10N

            var mmap = $@"PRAGMA mmap_size={dbSize};";
            var cache = $@"PRAGMA cache_size = {pages};";
            var createTable =$@"
                        CREATE TABLE IF NOT EXISTS Log (
                                 Ix INTEGER PRIMARY KEY,
                                 Header TEXT  NOT NULL,
                                 Body  TEXT
                                 {additionalCreate}
                        );
                    ";
            this.connection.ExecuteNonQuery(syncOff, journal, cache, temp, encode, mmap, createTable);
            this.Index();
            this.additionalColumns = this.ReadAdditionalColumns(c => c).ToArray();
            this.propertyTypesCache = new Dictionary<string, PropertyType>();
            foreach (var column in this.additionalColumns)
            {
                this.propertyTypesCache.Add(column, this.DefinePropertyType(column));
            }
            this.insertPrefix = $"INSERT INTO Log(Ix, Header, Body{this.CreateAdditionalColumnsList()}) VALUES (";
            this.insertSuffix = $", @Header, @Body {this.CreateAdditionalColumnsList("@")})"; // Not L10N
        }

        #endregion

        #region Public Methods and Operators

        public void Dispose()
        {
            this.Dispose(true);
        }

        [PublicAPI]
        public void StartAddMessages()
        {
            this.NoIndex();
            this.connection.BeginTran();
        }

        [PublicAPI]
        public void FinishAddMessages()
        {
            this.connection.CommitTran();
            this.Index();
        }

        [PublicAPI]
        public void Index()
        {
            this.connection.ExecuteNonQuery(this.CreateAdditionalIndexes().ToArray());
        }

        [PublicAPI]
        public void NoIndex()
        {
            this.connection.ExecuteNonQuery(this.DropAdditionalIndexes().ToArray());
        }

        [PublicAPI]
        public void AddMessage(LogMessage message)
        {
            message.Build(this.rules);

            void Action(IDbCommand command)
            {
                try
                {
                    command.AddParameter("@Header", message.Header); // Not L10N
                    command.AddParameter("@Body", message.Body); // Not L10N
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < this.additionalColumns.Length; i++)
                    {
                        var column = this.additionalColumns[i];
                        if (this.propertyTypesCache[column] == PropertyType.Integer)
                        {
                            command.AddParameter("@" + column, message.IntegerProperty(column)); // Not L10N
                        }
                        else
                        {
                            command.AddParameter("@" + column, message.StringProperty(column)); // Not L10N
                        }
                    }
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Log.Instance.Debug(e);
                }
            }

            // ugly but very fast
            this.connection.RunSqlQuery(Action, this.insertPrefix + message.Ix + this.insertSuffix);
        }

        [PublicAPI]
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
            var query = new StringBuilder();
            query.Append("SELECT Header, Body ");
            query.Append(this.CreateAdditionalColumnsList());
            query.Append(" FROM Log ");
            query.Append(this.Where(min, max, filter, useRegexp, start, finish));
            query.Append(" ORDER BY Ix ");
            query.Append(reverse ? "DESC" : "ASC");
            query.Append(" LIMIT ");
            query.Append(limit);
            query.Append(" OFFSET ");
            query.Append(offset);

            void BeforeRead(IDbCommand command) => this.AddParameters(command, min, max, filter, useRegexp, start, finish);

            void OnRead(IDataReader rdr)
            {
                var msg = new LogMessage(rdr[0] as string, rdr[1] as string);
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var ix = 0; ix < this.additionalColumns.Length; ix++)
                {
                    var column = this.additionalColumns[ix];
                    if (this.propertyTypesCache[column] == PropertyType.Integer)
                    {
                        msg.UpdateIntegerProperty(column, (long) rdr[column]);
                    }
                    else
                    {
                        msg.UpdateStringProperty(column, rdr[column] as string);
                    }
                }

                onReadMessage(msg);
            }

            this.connection.ExecuteReader(query.ToString(), OnRead, BeforeRead, notCancelled);
        }

        private PropertyType DefinePropertyType(string param)
        {
            var result = this.builder.DefineParserType(param);
            return result == ParserType.String ? PropertyType.String : PropertyType.Integer;
        }

        [PublicAPI]
        public long CountMessages(
            LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true,
            bool excludeNoLevel = false)
        {
            return this.CountMessages(DateTime.MinValue, DateTime.MaxValue, min, max, filter, useRegexp, excludeNoLevel);
        }

        [PublicAPI]
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

            var query = @"SELECT count(1) FROM Log " + this.Where(min, max, filter, useRegexp, start, finish);

            void AddParameters(IDbCommand cmd) => this.AddParameters(cmd, min, max, filter, useRegexp, start, finish);

            var result = this.connection.ExecuteScalar<long>(query, AddParameters);
            return result;
        }

        [PublicAPI]
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
            var query = $@"SELECT {func}({this.dateTimeProperty}) FROM Log {where}";

            void AddParameters(IDbCommand cmd) => this.AddParameters(cmd, min, max, filter, useRegexp, DateTime.MinValue, DateTime.MaxValue);

            var result = this.connection.ExecuteScalar<long>(query, AddParameters);

            return DateTime.FromFileTime(result);
        }

        public IEnumerable<KeyValuePair<LogLevel, long>> CountByLevel(string filter = null, bool useRegexp = true, bool excludeNoLevel = false)
        {
            return this.CountByLevel(DateTime.MinValue, DateTime.MaxValue, filter, useRegexp, excludeNoLevel);
        }

        public IEnumerable<KeyValuePair<LogLevel, long>> CountByLevel(DateTime start, DateTime finish, string filter = null, bool useRegexp = true, bool excludeNoLevel = false)
        {
            var levels = Enum.GetValues(typeof(LogLevel));

            return levels.Cast<LogLevel>()
                .Where(level => level != LogLevel.None)
                .ToDictionary(level => level, level => this.CountMessages(start, finish, level, level, filter, useRegexp, excludeNoLevel))
                .OrderBy(x => x.Key);
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
            return "WHERE " + string.Join(" AND ", notEmpty); // Not L10N
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
                clause.Add(this.logLevelProperty + " >= @Min"); // Not L10N
            }
            if (max != LogLevel.Fatal)
            {
                clause.Add(this.logLevelProperty + " <= @Max"); // Not L10N
            }
            return string.Join(" AND ", clause); // Not L10N
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
                clause.Add(this.dateTimeProperty + " >= @Start"); // Not L10N
            }
            if (finish != DateTime.MaxValue)
            {
                clause.Add(this.dateTimeProperty + " <= @Finish"); // Not L10N
            }
            return string.Join(" AND ", clause); // Not L10N
        }

        private static string FilterClause(string filter, bool useRegexp)
        {
            return string.IsNullOrWhiteSpace(filter) ? string.Empty : "(Header || Body) " + (useRegexp ? "REGEXP" : "LIKE") + " @Filter";
        }

        private string ExcludeNoLevelClause(bool excludeNoLevel)
        {
            return excludeNoLevel && this.hasLogLevelProperty ? this.logLevelProperty + " > " + (int)LogLevel.None : string.Empty; // Not L10N
        }

        private void AddParameters(IDbCommand command, LogLevel min, LogLevel max, string filter, bool useRegexp, DateTime start, DateTime finish)
        {
            if (this.hasLogLevelProperty)
            {
                if (min != LogLevel.Trace)
                {
                    command.AddParameter("@Min", (int) min); // Not L10N
                }
                if (max != LogLevel.Fatal)
                {
                    command.AddParameter("@Max", (int) max); // Not L10N
                }
            }

            if (this.hasDateTimeProperty)
            {
                if (start != DateTime.MinValue)
                {
                    command.AddParameter("@Start", start.ToFileTimeUtc()); // Not L10N
                }
                if (finish != DateTime.MaxValue)
                {
                    command.AddParameter("@Finish", finish.ToFileTimeUtc()); // Not L10N
                }
            }

            if (string.IsNullOrWhiteSpace(filter))
            {
                return;
            }
            var f = useRegexp ? filter : "%" + filter.Trim('%') + "%"; // Not L10N
            command.AddParameter("@Filter", f); // Not L10N
        }

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed",
            MessageId = "connection")]
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.connection?.Dispose();
                this.DeleteFile();
            }
        }

        private void DeleteFile()
        {
            while (true)
            {
                try
                {
                    if (File.Exists(this.DatabasePath))
                    {
                        File.Delete(this.DatabasePath);
                    }
                    return;
                }
                catch (IOException e)
                {
                    Log.Instance.Error(e.Message, e);
                    SpinWait.SpinUntil(() => this.DatabasePath.IsFileReady(), TimeSpan.FromSeconds(DelaySeconds));
                }
            }
        }
    }
}