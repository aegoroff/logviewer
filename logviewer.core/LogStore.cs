using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.Devices;

namespace logviewer.core
{
    public sealed class LogStore : IDisposable
    {
        #region Constants and Fields

        const string CreateIndexOnLevel = @"CREATE INDEX IX_Level ON Log (Level)";
        const int PageSize = 1024;
        private readonly DatabaseConnection connection;
        
        #endregion

        #region Constructors and Destructors

        public LogStore(long dbSize = 0L, string databaseFilePath = null)
        {
            DatabasePath = databaseFilePath ?? Path.GetTempFileName();
            connection = new DatabaseConnection(DatabasePath) ;

            const string CreateTable = @"
                        CREATE TABLE Log (
                                 Ix INTEGER PRIMARY KEY AUTOINCREMENT,
                                 Header TEXT  NOT NULL,
                                 Body  TEXT,
                                 Level INTEGER NOT NULL
                        );
                    ";
            
            const string SyncOff = @"PRAGMA synchronous = OFF;";
            const string Journal = @"PRAGMA journal_mode = MEMORY;";
            const string Cache = @"PRAGMA cache_size = {0};";
            const string Temp = @"PRAGMA temp_store = 2;";
            const string Encode = @"PRAGMA encoding = 'UTF-8';";
            const string Mmap = @"PRAGMA mmap_size={0};";

            var freePages = new ComputerInfo().AvailablePhysicalMemory / PageSize;
            var sqliteAvailablePages = (int)(freePages * 0.2);

            var pages = Math.Min(sqliteAvailablePages, dbSize / PageSize + 1);

            var mmap = string.Format(Mmap, dbSize);
            var cache = string.Format(Cache, pages);
            this.connection.ExecuteNonQuery(SyncOff, Journal, cache, Temp, Encode, mmap, CreateTable, CreateIndexOnLevel);
        }

        public string DatabasePath { get; private set; }

        #endregion

        #region Public Methods and Operators

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
            this.connection.ExecuteNonQuery(CreateIndexOnLevel);
        }
        
        public void NoIndex()
        {
            this.connection.ExecuteNonQuery(@"DROP INDEX IF EXISTS IX_Level");
        }

        public void AddMessage(LogMessage message)
        {
            const string Cmd = @"INSERT INTO Log(Header, Body, Level) VALUES (@Header, @Body, @Level)";
            this.connection.RunSqlQuery(delegate(SQLiteCommand command)
            {
                command.Parameters.AddWithValue("@Header", message.Header);
                command.Parameters.AddWithValue("@Body", message.Body);
                command.Parameters.AddWithValue("@Level", (int)message.Level);
                command.ExecuteNonQuery();
            }, Cmd);
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
            var query = string.Format(@"SELECT Header, Body, Level FROM Log {3} ORDER BY Ix {0} LIMIT {1} OFFSET {2}", order, limit, offset, where);
            this.connection.RunSqlQuery(delegate(SQLiteCommand command)
            {
                AddParameters(command, min, max, filter, useRegexp);
                var rdr = command.ExecuteReader();
                using (rdr)
                {
                    while (rdr.Read() && notCancelled())
                    {
                        var msg = new LogMessage(rdr[0] as string, rdr[1] as string, (LogLevel)((long)rdr[2]));
                        onReadMessage(msg);
                    }
                }
            }, query);
        }

        public long CountMessages(
            LogLevel min = LogLevel.Trace,
            LogLevel max = LogLevel.Fatal,
            string filter = null,
            bool useRegexp = true)
        {
            var result = 0L;
            var where = Where(min, max, filter, useRegexp);
            var query = string.Format(@"SELECT count(1) FROM Log {0}", where);
            this.connection.RunSqlQuery(delegate(SQLiteCommand command)
            {
                AddParameters(command, min, max, filter, useRegexp);
                result = (long)command.ExecuteScalar();
                
            }, query);

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
            var func = string.Format("(Header {0} @Filter OR Body {0} @Filter)", comparer);
            return string.IsNullOrWhiteSpace(filter) ? string.Empty : func;
        }

        private static void AddParameters(SQLiteCommand command, LogLevel min, LogLevel max, string filter, bool useRegexp)
        {
            if (min != LogLevel.Trace)
            {
                command.Parameters.AddWithValue("@Min", (int)min);
            }
            if (max != LogLevel.Fatal)
            {
                command.Parameters.AddWithValue("@Max", (int)max);
            }
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var f = useRegexp ? filter : string.Format("%{0}%", filter.Trim('%'));
                command.Parameters.AddWithValue("@Filter", f);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Methods

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
                if (File.Exists(DatabasePath))
                {
                    File.Delete(DatabasePath);
                }
            }
        }

        #endregion
    }

    [SQLiteFunction(Name = "REGEXP", Arguments = 2, FuncType = FunctionType.Scalar)]
    class SqliteRegEx : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            var input = Convert.ToString(args[1]);
            var pattern = Convert.ToString(args[0]);
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
    }
}