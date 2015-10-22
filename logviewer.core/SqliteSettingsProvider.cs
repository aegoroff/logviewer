// Created by: egr
// Created at: 02.05.2013
// © 2012-2015 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using logviewer.core.Properties;
using Microsoft.Win32;
using Net.Sgoliver.NRtfTree.Util;
using logviewer.engine;

namespace logviewer.core
{
    public sealed class SqliteSettingsProvider : ISettingsProvider
    {
        private const string RegistryKeyBase = @"Software\Egoroff\logviewer\";

        private const string OptionsSectionName = @"Options";
        private const string FilterParameterName = @"MessageFilter";
        private const string LastUpdateCheckTimearameterName = @"LastUpdateCheckTime";
        private const string OpenLastFileParameterName = @"OpenLastFile";
        private const string AutoRefreshOnFileChangeName = @"AutoRefreshOnFileChange";
        private const string MinLevelParameterName = @"MinLevel";
        private const string MaxLevelParameterName = @"MaxLevel";
        private const string SortingParameterName = @"Sorting";
        private const string PageSizeParameterName = @"PageSize";
        private const string SelectedTemplateParameterName = @"SelectedTemplate";
        private const string UseRegexpParameterName = @"UseRegexp";
        private const string KeepLastNFilesParameterName = @"KeepLastNFiles";
        private const int DefaultParsingProfileIndex = 0;
        private const string ApplicationOptionsFolder = "logviewer";
        private static readonly string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string applicationFolder = Path.Combine(baseFolder, ApplicationOptionsFolder);
        private readonly int defaultKeepLastNFiles;
        private readonly int defaultPageSize;
        private readonly string settingsDatabaseFilePath;
        private readonly IEnumerable<string> parsingTemplatePropertiesNames;
        private readonly IEnumerable<ColumnAttribute> parsingTemplatePropertiesColumns;
        private readonly IEnumerable<PropertyInfo> parsingTemplateProperties;
        private readonly List<Action<DatabaseConnection>> upgrades = new List<Action<DatabaseConnection>>();
        private readonly Dictionary<LogLevel, RtfCharFormat> bodyFormatsMap = new Dictionary<LogLevel, RtfCharFormat>();
        private readonly Dictionary<LogLevel, RtfCharFormat> headerFormatsMap = new Dictionary<LogLevel, RtfCharFormat>();

        private const int HeaderFontSize = 10;
        private const int BodyFontSize = 9;
        private const int KeepLastFilters = 20;

        private static readonly Dictionary<LogLevel, Color> defaultColors = new Dictionary<LogLevel, Color>
        {
            {LogLevel.None, Color.Black},
            {LogLevel.Trace, Color.FromArgb(200, 200, 200)},
            {LogLevel.Debug, Color.FromArgb(100, 100, 100)},
            {LogLevel.Info, Color.Green},
            {LogLevel.Warn, Color.Orange},
            {LogLevel.Error, Color.Red},
            {LogLevel.Fatal, Color.DarkViolet}
        };

        public SqliteSettingsProvider(string settingsDatabaseFileName,
            int defaultPageSize,
            int defaultKeepLastNFiles)
        {
            this.defaultPageSize = defaultPageSize;
            this.defaultKeepLastNFiles = defaultKeepLastNFiles;
            this.settingsDatabaseFilePath = Path.Combine(ApplicationFolder, settingsDatabaseFileName);
            this.parsingTemplateProperties = ReadParsingTemplateProperties().ToArray();
            this.parsingTemplatePropertiesColumns = this.parsingTemplateProperties.Select(GetColumnAttribute).ToArray();
            this.parsingTemplatePropertiesNames = this.parsingTemplatePropertiesColumns.Select(c => c.Name).ToArray();

            this.upgrades.Add(Upgrade1);
            this.upgrades.Add(Upgrade2);
            this.upgrades.Add(this.Upgrade3);
            this.upgrades.Add(this.Upgrade4);
            this.upgrades.Add(this.Upgrade5);

            this.CreateTables();
            this.MigrateFromRegistry();
            this.RunUpgrade();
            this.CacheFormats();
            var template = this.ReadParsingTemplate();
            if (!template.IsEmpty)
            {
                return;
            }

            foreach (var t in ParsingTemplate.Defaults)
            {
                this.InsertParsingTemplate(t);
            }
        }

        private void CacheFormats()
        {
            var levels = SettingsController.SelectLevels();
            foreach (var logLevel in levels)
            {
                var color = this.ReadColor(logLevel);
                this.headerFormatsMap.Add(logLevel, FormatChar(color, true));
                this.bodyFormatsMap.Add(logLevel, FormatChar(color, false, BodyFontSize));
            }
        }

        public Color ReadColor(LogLevel level)
        {
            var argb = this.ReadIntegerOption(level.ToParameterName(), -1);
            return (argb == -1) ? defaultColors[level] : Color.FromArgb(argb);
        }

        public IDictionary<LogLevel, Color> DefaultColors => defaultColors;

        public int SelectedParsingTemplate
        {
            get { return this.ReadIntegerOption(SelectedTemplateParameterName); }
            set { this.UpdateIntegerOption(SelectedTemplateParameterName, value); }
        }

        public void UpdateColor(LogLevel level, Color color)
        {
            if (!defaultColors.ContainsKey(level))
            {
                return;
            }
            this.headerFormatsMap[level] = FormatChar(color, true);
            this.bodyFormatsMap[level] = FormatChar(color, false, BodyFontSize);
            this.UpdateIntegerOption(level.ToParameterName(), color.ToArgb());
        }

        private static RegistryKey RegistryKey => GetRegKey(RegistryKeyBase + OptionsSectionName);

        private static bool MigrationNeeded => Registry.CurrentUser.OpenSubKey(RegistryKeyBase + OptionsSectionName, true) != null;

        public static string ApplicationFolder
        {
            get
            {
                if (!Directory.Exists(applicationFolder))
                {
                    Directory.CreateDirectory(applicationFolder);
                }
                return applicationFolder;
            }
        }

        public string MessageFilter
        {
            get { return this.ReadStringOption(FilterParameterName); }
            set { this.UpdateStringOption(FilterParameterName, value); }
        }

        public DateTime LastUpdateCheckTime
        {
            get { return DateTime.Parse(this.ReadStringOption(LastUpdateCheckTimearameterName, DateTime.UtcNow.ToString("O"))).ToUniversalTime(); }
            set { this.UpdateStringOption(LastUpdateCheckTimearameterName, value.ToString("O")); }
        }

        public bool OpenLastFile
        {
            get { return this.ReadBooleanOption(OpenLastFileParameterName); }
            set { this.UpdateBooleanOption(OpenLastFileParameterName, value); }
        }

        public bool AutoRefreshOnFileChange
        {
            get { return this.ReadBooleanOption(AutoRefreshOnFileChangeName); }
            set { this.UpdateBooleanOption(AutoRefreshOnFileChangeName, value); }
        }

        public int MinLevel
        {
            get { return this.ReadIntegerOption(MinLevelParameterName); }
            set { this.UpdateIntegerOption(MinLevelParameterName, value); }
        }

        public int MaxLevel
        {
            get { return this.ReadIntegerOption(MaxLevelParameterName, (int)LogLevel.Fatal); }
            set { this.UpdateIntegerOption(MaxLevelParameterName, value); }
        }

        public int PageSize
        {
            get { return this.ReadIntegerOption(PageSizeParameterName, this.defaultPageSize); }
            set { this.UpdateIntegerOption(PageSizeParameterName, value); }
        }

        public bool Sorting
        {
            get { return this.ReadBooleanOption(SortingParameterName); }
            set { this.UpdateBooleanOption(SortingParameterName, value); }
        }

        public bool UseRegexp
        {
            get { return this.ReadBooleanOption(UseRegexpParameterName); }
            set { this.UpdateBooleanOption(UseRegexpParameterName, value); }
        }

        public int KeepLastNFiles
        {
            get { return this.ReadIntegerOption(KeepLastNFilesParameterName, this.defaultKeepLastNFiles); }
            set { this.UpdateIntegerOption(KeepLastNFilesParameterName, value); }
        }

        public string FullPathToDatabase => this.settingsDatabaseFilePath;

        public void UpdateParsingTemplate(ParsingTemplate template)
        {
            var propertiesSet = string.Join(",", from string member in this.parsingTemplatePropertiesNames select string.Format("{0} = @{0}", member));

            this.ExecuteNonQuery(
                $@"
                    UPDATE
                        ParsingTemplates
                    SET
                        {
                    propertiesSet}
                    WHERE
                        Ix = @Ix
                    ", command => this.AddParsingTemplateIntoCommand(command, template));
        }

        private static IEnumerable<PropertyInfo> ReadParsingTemplateProperties()
        {
            return 
                from info in typeof(ParsingTemplate).GetProperties()
                where info.IsDefined(typeof(ColumnAttribute), false)
                select info;
        }

        public ParsingTemplate ReadParsingTemplate()
        {
            return this.ReadParsingTemplate(this.SelectedParsingTemplate);
        }

        public IList<string> ReadParsingTemplateList()
        {
            const string cmd = @"
                    SELECT
                        Name
                    FROM
                        ParsingTemplates
                    ORDER BY Ix
                    ";

            var result = new List<string>();

            Action<IDataReader> onRead = rdr => result.Add(rdr[0] as string);
            Action<DatabaseConnection> action = connection => connection.ExecuteReader(onRead, cmd);

            this.ExecuteQuery(action);

            return result;
        }

        public IList<ParsingTemplate> ReadAllParsingTemplates()
        {
            const string cmd = @"
                    SELECT
                        Ix,                        
                        Name,
                        StartMessage
                    FROM
                        ParsingTemplates
                    ORDER BY Ix
                    ";

            var result = new List<ParsingTemplate>();

            Action<IDataReader> onRead = rdr => result.Add(new ParsingTemplate { Index = (int)((long)rdr[0]), Name = rdr[1] as string, StartMessage = rdr[2] as string });
            Action<DatabaseConnection> action = connection => connection.ExecuteReader(onRead, cmd);

            this.ExecuteQuery(action);

            return result;
        }

        public ParsingTemplate ReadParsingTemplate(int index)
        {
            var propertiesGet = string.Join(",", from level in this.parsingTemplatePropertiesNames select level);

            var result = new ParsingTemplate { Index = index };

            Action<IDbCommand> beforeRead = command => DatabaseConnection.AddParameter(command, "@Ix", index);
           
            Action<IDataReader> onRead = delegate(IDataReader rdr)
            {
                foreach (var column in parsingTemplateProperties)
                {
                    var attr = GetColumnAttribute(column);
                    if (column.PropertyType == typeof(bool))
                    {
                        var v = rdr[attr.Name];
                        column.SetValue(result, (bool)v, null);
                    }
                    else
                    {
                        column.SetValue(result, rdr[attr.Name] as string, null);
                    }
                }
            };

            var query =
                    $@"
                    SELECT
                        {propertiesGet}
                    FROM
                        ParsingTemplates
                    WHERE
                        Ix = @Ix
                    ";
            Action<DatabaseConnection> action = connection => connection.ExecuteReader(onRead, query, beforeRead);
            this.ExecuteQuery(action);

            return result;
        }

        private static ColumnAttribute GetColumnAttribute(PropertyInfo column)
        {
            return (ColumnAttribute) column.GetCustomAttributes(typeof (ColumnAttribute), false)[0];
        }

        public void InsertParsingTemplate(ParsingTemplate template)
        {
            var propertiesColumns = string.Join(",", from level in this.parsingTemplatePropertiesNames select level);
            var propertiesParams = string.Join(",", from level in this.parsingTemplatePropertiesNames select "@" + level);

            var query =
                    $@"
                    INSERT INTO ParsingTemplates (
                        Ix, 
                        {propertiesColumns}
                    )
                    VALUES (
                        @Ix,
                        {propertiesParams}
                    )";
            this.ExecuteNonQuery(query, command => this.AddParsingTemplateIntoCommand(command, template));
        }
        
        public void DeleteParsingTemplate(int ix)
        {
            var query = $@"DELETE FROM ParsingTemplates WHERE Ix = {ix}";

            const string selectIndexesCmd = @"
                    SELECT
                        Ix
                    FROM
                        ParsingTemplates
                    WHERE Ix > @Ix
                    ORDER BY Ix
                    ";
            
            const string updateIndexesCmd = @"
                    UPDATE
                        ParsingTemplates
                    SET
                        Ix = @NewIx
                    WHERE
                        Ix = @Ix
                    ";

            var indexesToUpdate = new List<long>();

            Action<IDbCommand> beforeRead = command => DatabaseConnection.AddParameter(command, "@Ix", ix);

            Action<IDataReader> onRead = delegate(IDataReader rdr)
            {
                if (rdr[0] is DBNull)
                {
                    return;
                }
                indexesToUpdate.Add((long)rdr[0]);
            };

            this.ExecuteQuery(delegate(DatabaseConnection connection)
            {
                connection.BeginTran();
                try
                {
                    connection.ExecuteNonQuery(query);
                    connection.ExecuteReader(onRead, selectIndexesCmd, beforeRead);

                    foreach (var beforeUpdate in indexesToUpdate.Select(index => (Action<IDbCommand>)delegate(IDbCommand command)
                    {
                        DatabaseConnection.AddParameter(command, "@Ix", index);
                        DatabaseConnection.AddParameter(command, "@NewIx", index - 1);
                    }))
                    {
                        connection.ExecuteNonQuery(updateIndexesCmd, beforeUpdate);
                    }
                }
                catch (Exception)
                {
                    connection.RollbackTran();
                    throw;
                }
                connection.CommitTran();
            });
        }

        public RtfCharFormat FormatHead(LogLevel level)
        {
            return this.headerFormatsMap[level];
        }

        public RtfCharFormat FormatBody(LogLevel level)
        {
            return this.bodyFormatsMap[level];
        }

        public void UseRecentFilesStore(Action<RecentItemsStore> action)
        {
            this.UseRecentItemsStore(action, "RecentFiles");
        }

        public void UseRecentFiltersStore(Action<RecentItemsStore> action)
        {
            this.UseRecentItemsStore(action, "RecentFilters", KeepLastFilters);
        }

        private static RtfCharFormat FormatChar(Color color, bool bold, int size = HeaderFontSize)
        {
            return new RtfCharFormat
            {
                Color = color,
                Font = "Courier New",
                Size = size,
                Bold = bold
            };
        }

        private void CreateTables()
        {
            const string optionsTableTemplate = @"
                        CREATE TABLE IF NOT EXISTS {0} (
                                 Option TEXT PRIMARY KEY,
                                 Value {1}
                        );
                    ";
            
            var stringOptions = string.Format(optionsTableTemplate, "StringOptions", "TEXT");
            var integerOptions = string.Format(optionsTableTemplate, "IntegerOptions", "INTEGER");
            var booleanOptions = string.Format(optionsTableTemplate, "BooleanOptions", "BOOLEAN");

            const string databaseConfigurationTable = @"
                        CREATE TABLE IF NOT EXISTS DatabaseConfiguration (
                                 Version INTEGER PRIMARY KEY,
                                 OccurredAt INTEGER  NOT NULL
                        );
                    ";

            this.ExecuteNonQuery(stringOptions, integerOptions, booleanOptions, this.ParsingTeplateCreateCmd(), databaseConfigurationTable);
        }

        private string ParsingTeplateCreateCmd()
        {
            var propertiesCreate = string.Join(", ", this.ParsingTemplateColumnsDefinition());
            return $@"CREATE TABLE IF NOT EXISTS ParsingTemplates (
                                 Ix INTEGER PRIMARY KEY,
                                 {propertiesCreate}
                        );
                    ";
        }

        IEnumerable<string> ParsingTemplateColumnsDefinition()
        {
            var q = from ColumnAttribute member in this.parsingTemplatePropertiesColumns select member;
            foreach (var c in q)
            {
                if (c.Name == "Compiled")
                {
                    yield return c.Name + " BOOLEAN NULL DEFAULT FALSE";
                }
                else
                {
                    var n = c.Nullable ? string.Empty : "NOT";
                    yield return c.Name + " TEXT " + n +" NULL";
                }
            }
        }

        private void RunUpgrade()
        {
            var since = (int)this.SchemaVersion;

            this.ExecuteQuery(delegate(DatabaseConnection connection)
            {
                connection.BeginTran();
                try
                {
                    for (var i = since; i < this.upgrades.Count; i++)
                    {
                        this.upgrades[i](connection);
                        InsertSchemaVersion(i + 1, connection);
                    }
                }
                catch (Exception)
                {
                    connection.RollbackTran();
                    throw;
                }
                connection.CommitTran();
            });
        }

        private long SchemaVersion
        {
            get
            {
                try
                {
                    return this.ExecuteScalar<long>(@"SELECT max(Version) FROM DatabaseConfiguration");
                }
                catch (Exception e)
                {
                    Log.Instance.Debug(e.Message, e);
                }
                return 0;
            }
        }

        private static void InsertSchemaVersion(long version, DatabaseConnection connection)
        {
            Action<IDbCommand> action = delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Version", version);
                DatabaseConnection.AddParameter(command, "@OccurredAt", DateTime.Now.Ticks);
            };

            connection.ExecuteNonQuery(@"INSERT INTO DatabaseConfiguration (Version, OccurredAt) VALUES (@Version, @OccurredAt)", action);
        }

        static void Upgrade1(DatabaseConnection connection)
        {
            var result = connection.ExecuteScalar<long>(@"SELECT count(1) FROM ParsingTemplates");
            if (result <= 0)
            {
                return;
            }
            connection.ExecuteNonQuery(@"ALTER TABLE ParsingTemplates ADD COLUMN Name TEXT");
            const string cmd = @"
                    UPDATE
                        ParsingTemplates
                    SET
                        Name = @Name
                    WHERE
                        Ix = @Ix
                    ";

            Action<IDbCommand> action = delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Ix", DefaultParsingProfileIndex);
                DatabaseConnection.AddParameter(command, "@Name", Resources.ParsingTemplateNlog);
            };

            connection.ExecuteNonQuery(cmd, action);
        }
        
        static void Upgrade2(DatabaseConnection connection)
        {
            connection.ExecuteNonQuery(@"DROP TABLE IF EXISTS RecentFiles");
        }
        
        void Upgrade3(DatabaseConnection connection)
        {
            var properties = string.Join(",",
                from string member in this.parsingTemplatePropertiesNames select member);

            this.UpgradeParsingTemplatesTable(connection, properties);
        }
        
        void Upgrade4(DatabaseConnection connection)
        {
            var properties = string.Join(",",
                from string member in this.parsingTemplatePropertiesNames.Where(n => !n.Equals("Filter", StringComparison.Ordinal)) select member);

            this.UpgradeParsingTemplatesTable(connection, properties);
        }
        
        void Upgrade5(DatabaseConnection connection)
        {
            var properties = string.Join(",",
                from string member in this.parsingTemplatePropertiesNames.Where(n => !n.Equals("Compiled", StringComparison.Ordinal)) select member);

            this.UpgradeParsingTemplatesTable(connection, properties);
        }

        private void UpgradeParsingTemplatesTable(DatabaseConnection connection, string properties)
        {
            properties = "Ix," + properties;

            connection.ExecuteNonQuery(@"ALTER TABLE ParsingTemplates RENAME TO ParsingTemplatesOld");
            connection.ExecuteNonQuery(this.ParsingTeplateCreateCmd());
            connection.ExecuteNonQuery(@"INSERT INTO ParsingTemplates(" + properties + ") SELECT " + properties +
                                       " FROM ParsingTemplatesOld");
            connection.ExecuteNonQuery(@"DROP TABLE ParsingTemplatesOld");
        }

        private void ExecuteNonQuery(params string[] queries)
        {
            this.ExecuteQuery(connection => connection.RunSqlQuery(command => command.ExecuteNonQuery(), queries));
        }

        private T ExecuteScalar<T>(string query, Action<IDbCommand> actionBeforeExecute = null)
        {
            var result = default(T);
            Action<DatabaseConnection> action = delegate(DatabaseConnection connection)
            {
                result = connection.ExecuteScalar<T>(query, actionBeforeExecute);
            };
            this.ExecuteQuery(action);
            return result;
        }

        private void ExecuteNonQuery(string query, Action<IDbCommand> actionBeforeExecute = null)
        {
            this.ExecuteQuery(connection => connection.ExecuteNonQuery(query, actionBeforeExecute));
        }
        
        private void ExecuteQuery(Action<DatabaseConnection> action)
        {
            var connection = new DatabaseConnection(this.settingsDatabaseFilePath);
            using (connection)
            {
                action(connection);
            }
        }

        private void MigrateFromRegistry()
        {
            if (!MigrationNeeded)
            {
                return;
            }
            this.MigrateString(FilterParameterName);

            var boolOptions = new[]
            {
                UseRegexpParameterName, OpenLastFileParameterName, SortingParameterName
            };
            foreach (var opt in boolOptions)
            {
                this.MigrateBoolean(opt);
            }

            var intOptions = new[]
            {
                MinLevelParameterName, MaxLevelParameterName, PageSizeParameterName, KeepLastNFilesParameterName
            };
            foreach (var opt in intOptions)
            {
                this.MigrateInteger(opt);
            }

            var template = new ParsingTemplate
            {
                Index = DefaultParsingProfileIndex,
                StartMessage = GetStringValue("StartMessageTemplate")
            };

            this.InsertParsingTemplate(template);

            Registry.CurrentUser.DeleteSubKeyTree(RegistryKeyBase);
        }

        void MigrateString(string option)
        {
            this.UpdateStringOption(option, GetStringValue(option));
        }
        
        void MigrateBoolean(string option)
        {
            this.UpdateBooleanOption(option, GetBoolValue(option));
        }
        
        void MigrateInteger(string option)
        {
            this.UpdateIntegerOption(option, GetIntValue(option));
        }

        private void UpdateStringOption(string option, string value)
        {
            this.UpdateOption("StringOptions", option, value);
        }

        private void UpdateBooleanOption(string option, bool value)
        {
            this.UpdateOption("BooleanOptions", option, value);
        }

        private void UpdateIntegerOption(string option, int value)
        {
            this.UpdateOption("IntegerOptions", option, value);
        }

        private void UpdateOption<T>(string table, string option, T value)
        {
            const string insertCmd = @"INSERT INTO {0}(Option, Value) VALUES (@Option, @Value)";

            const string updateCmd = @"
                    UPDATE
                        {0}
                    SET
                        Value = @Value
                    WHERE
                        Option = @Option
                    ";

            var query = $@"SELECT count(1) FROM {table} WHERE Option = @Option";
            var exist = this.ExecuteScalar<long>(query,
                command => DatabaseConnection.AddParameter(command, "@Option", option)) > 0;


            Action<IDbCommand> action = delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Option", option);
                DatabaseConnection.AddParameter(command, "@Value", value);
            };

            this.ExecuteNonQuery(string.Format(exist ? updateCmd : insertCmd, table), action);
        }

        private string ReadStringOption(string option, string defaultValue = null)
        {
            return this.ReadOption("StringOptions", option, defaultValue);
        }

        private bool ReadBooleanOption(string option, bool defaultValue = false)
        {
            return this.ReadOption("BooleanOptions", option, defaultValue);
        }

        private int ReadIntegerOption(string option, int defaultValue = 0)
        {
            return (int)this.ReadOption<long>("IntegerOptions", option, defaultValue);
        }

        private T ReadOption<T>(string table, string option, T defaultValue = default(T))
        {
            const string cmd = @"SELECT Value FROM {0} WHERE Option = @Option";
            var result = default(T);
            var read = false;

            Action<IDataReader> onRead = delegate(IDataReader rdr)
            {
                if (rdr[0] is DBNull)
                {
                    return;
                }
                result = (T)rdr[0];
                read = true;
            };

            Action<IDbCommand> beforeRead = command => DatabaseConnection.AddParameter(command, "@Option", option);
            Action<DatabaseConnection> action = connection => connection.ExecuteReader(onRead, string.Format(cmd, table), beforeRead);
            this.ExecuteQuery(action);


            if (read)
            {
                return result;
            }
            this.UpdateOption(table, option, defaultValue);
            result = defaultValue;
            return result;
        }

        private void AddParsingTemplateIntoCommand(IDbCommand command, ParsingTemplate template)
        {
            Action<string, object> action = (name, value) => DatabaseConnection.AddParameter(command, name, value);

            action("@Ix", template.Index);

            foreach (var column in this.parsingTemplateProperties)
            {
                var value = column.GetValue(template, null);
                var attr = GetColumnAttribute(column);
                action("@" + attr.Name, value);
            }
        }

        /// <summary>
        ///     Open/create regkey for keeping data
        /// </summary>
        /// <param name="key">key name</param>
        /// <returns>Exist or new RegistryKey object</returns>
        private static RegistryKey GetRegKey(string key)
        {
            return Registry.CurrentUser.OpenSubKey(key, true) ?? Registry.CurrentUser.CreateSubKey(key);
        }

        private static T GetValue<T>(RegistryKey rk, string key, T defaultValue = default(T))
        {
            var obj = rk.GetValue(key);
            return obj is T ? (T)obj : defaultValue;
        }

        private static string GetStringValue(RegistryKey rk, string key)
        {
            return GetValue(rk, key, string.Empty);
        }

        private static int GetIntValue(RegistryKey rk, string key)
        {
            return GetValue(rk, key, 0);
        }

        private static string GetStringValue(string key)
        {
            return GetStringValue(RegistryKey, key);
        }

        private static int GetIntValue(string key)
        {
            return GetIntValue(RegistryKey, key);
        }

        private static bool GetBoolValue(string key)
        {
            return GetIntValue(key) == 1;
        }

        private void UseRecentItemsStore(Action<RecentItemsStore> action, string table, int maxItems = 0)
        {
            try
            {
                using (var itemsStore = new RecentItemsStore(this, table, maxItems))
                {
                    action(itemsStore);
                }
            }
            catch (Exception e)
            {
                Log.Instance.Debug(e);
            }
        }
    }
}