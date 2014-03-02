// Created by: egr
// Created at: 02.05.2013
// © 2012-2014 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace logviewer.core
{
    public sealed class SqliteSettingsProvider : ISettingsProvider
    {
        private const string RegistryKeyBase = @"Software\Egoroff\logviewer\";

        private const string OptionsSectionName = @"Options";
        private const string FilterParameterName = @"MessageFilter";
        private const string OpenLastFileParameterName = @"OpenLastFile";
        private const string AutoRefreshOnFileChangeName = @"AutoRefreshOnFileChange";
        private const string MinLevelParameterName = @"MinLevel";
        private const string MaxLevelParameterName = @"MaxLevel";
        private const string SortingParameterName = @"Sorting";
        private const string PageSizeParameterName = @"PageSize";
        private const string UseRegexpParameterName = @"UseRegexp";
        private const string KeepLastNFilesParameterName = @"KeepLastNFiles";
        private const int DefaultParsingProfileIndex = 0;
        private const string DefaultParsingProfileName = "default";
        private const string ApplicationOptionsFolder = "logviewer";
        private static readonly string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string applicationFolder = Path.Combine(baseFolder, ApplicationOptionsFolder);
        private readonly int defaultKeepLastNFiles;
        private readonly int defaultPageSize;
        private readonly string settingsDatabaseFilePath;
        private readonly IEnumerable<string> parsingTemplatePropertiesNames;
        private readonly IEnumerable<PropertyInfo> parsingTemplateProperties;
        private readonly List<Action<DatabaseConnection>> upgrades = new List<Action<DatabaseConnection>>();

        public SqliteSettingsProvider(string settingsDatabaseFileName,
            IEnumerable<string> defaultLeveles,
            string defaultStartMessageTemplate,
            int defaultPageSize,
            int defaultKeepLastNFiles)
        {
            this.defaultPageSize = defaultPageSize;
            this.defaultKeepLastNFiles = defaultKeepLastNFiles;
            this.settingsDatabaseFilePath = Path.Combine(ApplicationFolder, settingsDatabaseFileName);
            this.parsingTemplateProperties = ReadParsingTemplateProperties().ToArray();
            this.parsingTemplatePropertiesNames = this.parsingTemplateProperties.Select(info => GetColumnAttribute(info).Name).ToArray();
            this.upgrades.Add(Upgrade1);

            this.CreateTables();
            this.MigrateFromRegistry();
            this.RunUpgrade();
            var template = this.ReadParsingTemplate();
            if (!template.IsEmpty)
            {
                return;
            }
            var defLevels = defaultLeveles.ToArray();
            var defaultTemplate = new ParsingTemplate
            {
                Index = DefaultParsingProfileIndex,
                StartMessage = defaultStartMessageTemplate,
                Name = DefaultParsingProfileName
            };
            for (var i = 0; i < defLevels.Length; i++)
            {
                defaultTemplate.UpdateLevelProperty(defLevels[i], (LogLevel)i);
            }

            this.InsertParsingProfile(defaultTemplate);
        }

        private static RegistryKey RegistryKey
        {
            get { return GetRegKey(RegistryKeyBase + OptionsSectionName); }
        }

        private static bool MigrationNeeded
        {
            get { return Registry.CurrentUser.OpenSubKey(RegistryKeyBase + OptionsSectionName, true) != null; }
        }

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

        public string FullPathToDatabase
        {
            get { return this.settingsDatabaseFilePath; }
        }

        public void UpdateParsingProfile(ParsingTemplate template)
        {
            var propertiesSet = string.Join(",", from string member in parsingTemplatePropertiesNames select string.Format("{0} = @{0}", member));

            const string Cmd = @"
                    UPDATE
                        ParsingTemplates
                    SET
                        {0}
                    WHERE
                        Ix = @Ix
                    ";
            this.ExecuteNonQuery(string.Format(Cmd, propertiesSet), command => AddParsingTemplateIntoCommand(command, template));
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
            return this.ReadParsingTemplate(DefaultParsingProfileIndex);
        }

        public IList<string> ReadParsingTemplateList()
        {
            const string Cmd = @"
                    SELECT
                        Name
                    FROM
                        ParsingTemplates
                    ORDER BY Ix
                    ";

            var result = new List<string>();

            Action<IDataReader> onRead = rdr => result.Add(rdr[0] as string);
            Action<DatabaseConnection> action = connection => connection.ExecuteReader(onRead, Cmd);

            ExecuteQuery(action);

            return result;
        }

        public ParsingTemplate ReadParsingTemplate(int index)
        {
            var propertiesGet = string.Join(",", from level in parsingTemplatePropertiesNames select level);
            
            const string Cmd = @"
                    SELECT
                        {0}
                    FROM
                        ParsingTemplates
                    WHERE
                        Ix = @Ix
                    ";

            var result = new ParsingTemplate { Index = index };

            Action<IDbCommand> beforeRead = command => DatabaseConnection.AddParameter(command, "@Ix", index);
           
            Action<IDataReader> onRead = delegate(IDataReader rdr)
            {
                foreach (var column in parsingTemplateProperties)
                {
                    var attr = GetColumnAttribute(column);
                    column.SetValue(result, rdr[attr.Name], null);
                }
            };

            var query = string.Format(Cmd, propertiesGet);
            Action<DatabaseConnection> action = connection => connection.ExecuteReader(onRead, query, beforeRead);
            ExecuteQuery(action);

            return result;
        }

        private static ColumnAttribute GetColumnAttribute(PropertyInfo column)
        {
            return (ColumnAttribute) column.GetCustomAttributes(typeof (ColumnAttribute), false)[0];
        }

        public void InsertParsingProfile(ParsingTemplate template)
        {
            var propertiesColumns = string.Join(",", from level in parsingTemplatePropertiesNames select level);
            var propertiesParams = string.Join(",", from level in parsingTemplatePropertiesNames select "@" + level);
            
            const string Cmd = @"
                    INSERT INTO ParsingTemplates (
                        Ix, 
                        {0}
                    )
                    VALUES (
                        @Ix,
                        {1}
                    )";
            var query = string.Format(Cmd, propertiesColumns, propertiesParams);
            this.ExecuteNonQuery(query, command => AddParsingTemplateIntoCommand(command, template));
        }

        private void CreateTables()
        {
            const string OptionsTableTemplate = @"
                        CREATE TABLE IF NOT EXISTS {0} (
                                 Option TEXT PRIMARY KEY,
                                 Value {1}
                        );
                    ";
            
            var stringOptions = string.Format(OptionsTableTemplate, "StringOptions", "TEXT");
            var integerOptions = string.Format(OptionsTableTemplate, "IntegerOptions", "INTEGER");
            var booleanOptions = string.Format(OptionsTableTemplate, "BooleanOptions", "BOOLEAN");

            var propertiesCreate = string.Join(",", from string member in parsingTemplatePropertiesNames select string.Format("{0} TEXT NOT NULL", member));
            const string ParsingTemplates = @"
                        CREATE TABLE IF NOT EXISTS ParsingTemplates (
                                 Ix INTEGER PRIMARY KEY,
                                 {0}
                        );
                    ";

            const string DatabaseConfigurationTable = @"
                        CREATE TABLE IF NOT EXISTS DatabaseConfiguration (
                                 Version INTEGER PRIMARY KEY,
                                 OccurredAt INTEGER  NOT NULL
                        );
                    ";

            this.ExecuteNonQuery(stringOptions, integerOptions, booleanOptions, string.Format(ParsingTemplates, propertiesCreate), DatabaseConfigurationTable);
        }

        private void RunUpgrade()
        {
            var since = (int)this.SchemaVersion;

            ExecuteQuery(delegate(DatabaseConnection connection)
            {
                connection.BeginTran();
                for (var i = since; i < this.upgrades.Count; i++)
                {
                    this.upgrades[i](connection);
                    InsertSchemaVersion(i + 1, connection);
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
            const string Cmd = @"
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
                DatabaseConnection.AddParameter(command, "@Name", DefaultParsingProfileName);
            };

            connection.ExecuteNonQuery(Cmd, action);
        }

        private void ExecuteNonQuery(params string[] queries)
        {
            ExecuteQuery(connection => connection.RunSqlQuery(command => command.ExecuteNonQuery(), queries));
        }

        private T ExecuteScalar<T>(string query, Action<IDbCommand> actionBeforeExecute = null)
        {
            var result = default(T);
            Action<DatabaseConnection> action = delegate(DatabaseConnection connection)
            {
                result = connection.ExecuteScalar<T>(query, actionBeforeExecute);
            };
            ExecuteQuery(action);
            return result;
        }

        private void ExecuteNonQuery(string query, Action<IDbCommand> actionBeforeExecute = null)
        {
            ExecuteQuery(connection => connection.ExecuteNonQuery(query, actionBeforeExecute));
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
            var parameters = new[]
            {
                "TraceLevel",
                "DebugLevel",
                "InfoLevel",
                "WarnLevel",
                "ErrorLevel",
                "FatalLevel"
            };
            for (var i = 0; i < parameters.Length; i++)
            {
                template.UpdateLevelProperty(parameters[i], (LogLevel)i);
            }

            this.InsertParsingProfile(template);

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
            const string InsertCmd = @"INSERT INTO {0}(Option, Value) VALUES (@Option, @Value)";

            const string UpdateCmd = @"
                    UPDATE
                        {0}
                    SET
                        Value = @Value
                    WHERE
                        Option = @Option
                    ";

            var query = string.Format(@"SELECT count(1) FROM {0} WHERE Option = @Option", table);
            var exist = this.ExecuteScalar<long>(query,
                command => DatabaseConnection.AddParameter(command, "@Option", option)) > 0;


            Action<IDbCommand> action = delegate(IDbCommand command)
            {
                DatabaseConnection.AddParameter(command, "@Option", option);
                DatabaseConnection.AddParameter(command, "@Value", value);
            };

            ExecuteNonQuery(string.Format(exist ? UpdateCmd : InsertCmd, table), action);
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
            const string Cmd = @"SELECT Value FROM {0} WHERE Option = @Option";
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
            Action<DatabaseConnection> action = connection => connection.ExecuteReader(onRead, string.Format(Cmd, table), beforeRead);
            ExecuteQuery(action);


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

            foreach (var column in parsingTemplateProperties)
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
    }
}