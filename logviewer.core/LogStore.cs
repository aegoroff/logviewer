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

        private SQLiteTransaction transaction;
        private readonly SQLiteConnection connection;
        const string CreateIndexOnLevel = @"CREATE INDEX IX_Level ON Log (Level)";
        const int PageSize = 1024;
        
        #endregion

        #region Constructors and Destructors

        public LogStore(long dbSize = 0L, string databaseFilePath = null)
        {
            DatabasePath = databaseFilePath ?? Path.GetTempFileName();
            SQLiteConnection.CreateFile(DatabasePath);
            var conString = new SQLiteConnectionStringBuilder { DataSource = DatabasePath };
            connection = new SQLiteConnection(conString.ToString());
            connection.Open();

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
            RunSqlQuery(command => command.ExecuteNonQuery(), SyncOff, Journal, cache, Temp, Encode, mmap, CreateTable, CreateIndexOnLevel);
        }

        public string DatabasePath { get; private set; }

        #endregion

        #region Public Methods and Operators

        public void StartAddMessages()
        {
            this.NoIndex();
            this.transaction = this.connection.BeginTransaction();
        }

        public void FinishAddMessages()
        {
            this.transaction.Commit();
            this.Index();
        }

        public void Index()
        {
            this.RunSqlQuery(command => command.ExecuteNonQuery(), CreateIndexOnLevel);
        }
        
        public void NoIndex()
        {
            this.RunSqlQuery(command => command.ExecuteNonQuery(), @"DROP INDEX IF EXISTS IX_Level");
        }

        public void AddMessage(LogMessage message)
        {
            const string Cmd = @"INSERT INTO Log(Header, Body, Level) VALUES (@Header, @Body, @Level)";
            RunSqlQuery(delegate(SQLiteCommand command)
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
            this.RunSqlQuery(delegate(SQLiteCommand command)
            {
                AddParameters(command, min, max, filter);
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
            this.RunSqlQuery(delegate(SQLiteCommand command)
            {
                AddParameters(command, min, max, filter);
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
            var func = useRegexp ? "(Header REGEXP @Fiter OR Body REGEXP @Fiter)" : "(Header LIKE @Fiter OR Body LIKE @Fiter)";
            return string.IsNullOrWhiteSpace(filter) ? string.Empty : func;
        }

        private static void AddParameters(SQLiteCommand command, LogLevel min, LogLevel max, string filter)
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
                command.Parameters.AddWithValue("@Fiter", filter);
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
                if (transaction != null)
                {
                    transaction.Dispose();
                }
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

        private void RunSqlQuery(Action<SQLiteCommand> action, params string[] commands)
        {
            foreach (var command in commands)
            {
                using (var sqLiteCommand = connection.CreateCommand())
                {
                    sqLiteCommand.CommandText = command;
                    action(sqLiteCommand);
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
    
    [SQLiteFunction(Name = "SUBSTR", Arguments = 2, FuncType = FunctionType.Scalar)]
    class Substring : SQLiteFunction
    {
        public override object Invoke(object[] args)
        {
            var input = Convert.ToString(args[0]);
            var pattern = Convert.ToString(args[1]);
            return input.Contains(pattern);
        }
    }
}