﻿// Created by: egr
// Created at: 02.05.2013
// © 2012-2013 Alexander Egorov

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace logviewer.core
{
    public sealed class SqliteSettingsProvider : ISettingsProvider
    {
        private const string RegistryKeyBase = @"Software\Egoroff\logviewer\";

        private const string OptionsSectionName = @"Options";
        private const string FilterParameterName = @"MessageFilter";
        private const string OpenLastFileParameterName = @"OpenLastFile";
        private const string MinLevelParameterName = @"MinLevel";
        private const string MaxLevelParameterName = @"MaxLevel";
        private const string SortingParameterName = @"Sorting";
        private const string PageSizeParameterName = @"PageSize";
        private const string UseRegexpParameterName = @"UseRegexp";
        private const string StartMessageParameterName = @"StartMessageTemplate";
        private const string TraceParameterName = @"TraceLevel";
        private const string DebugParameterName = @"DebugLevel";
        private const string InfoParameterName = @"InfoLevel";
        private const string WarnParameterName = @"WarnLevel";
        private const string ErrorParameterName = @"ErrorLevel";
        private const string FatalParameterName = @"FatalLevel";
        private const string KeepLastNFilesParameterName = @"KeepLastNFiles";
        private const int DefaultParsingProfileIndex = 0;
        private const string ApplicationOptionsFolder = "logviewer";
        private static readonly string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string applicationFolder = Path.Combine(baseFolder, ApplicationOptionsFolder);
        private readonly int defaultKeepLastNFiles;
        private readonly int defaultPageSize;
        private readonly string settingsDatabaseFilePath;

        public SqliteSettingsProvider(string settingsDatabaseFileName,
            IEnumerable<string> defaultLeveles,
            string defaultStartMessageTemplate,
            int defaultPageSize,
            int defaultKeepLastNFiles)
        {
            this.defaultPageSize = defaultPageSize;
            this.defaultKeepLastNFiles = defaultKeepLastNFiles;
            this.settingsDatabaseFilePath = Path.Combine(ApplicationFolder, settingsDatabaseFileName);
            this.CreateTables();
            this.MigrateFromRegistry();
            var template = this.ReadParsingTemplate();
            if (!template.IsEmpty)
            {
                return;
            }
            var levels = defaultLeveles.ToArray();
            var defaultTemplate = new ParsingTemplate
            {
                Index = DefaultParsingProfileIndex,
                StartMessage = defaultStartMessageTemplate,
                Trace = levels[(int)LogLevel.Trace],
                Debug = levels[(int)LogLevel.Debug],
                Info = levels[(int)LogLevel.Info],
                Warn = levels[(int)LogLevel.Warn],
                Error = levels[(int)LogLevel.Error],
                Fatal = levels[(int)LogLevel.Fatal]
            };
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
            const string Cmd = @"
                    UPDATE
                        ParsingTemplates
                    SET
                        StartMessage = @StartMessage,
                        Trace = @Trace,
                        Debug = @Debug,
                        Info = @Info,
                        Warn = @Warn,
                        Error = @Error,
                        Fatal = @Fatal
                    WHERE
                        Ix = @Ix
                    ";
            this.ExecuteNonQuery(Cmd, command => AddParsingTemplateIntoCommand(command, template));
        }

        public ParsingTemplate ReadParsingTemplate()
        {
            return this.ReadParsingTemplate(DefaultParsingProfileIndex);
        }

        public ParsingTemplate ReadParsingTemplate(int index)
        {
            const string Cmd = @"
                    SELECT
                        Trace,
                        Debug,
                        Info,
                        Warn,
                        Error,
                        Fatal,
                        StartMessage
                    FROM
                        ParsingTemplates
                    WHERE
                        Ix = @Ix
                    ";

            var result = new ParsingTemplate { Index = index };

            Action<IDbCommand> beforeRead = command => DatabaseConnection.AddParameter(command, "@Ix", index);
            
            Action<IDataReader> onRead = delegate(IDataReader rdr)
            {
                result.Trace = rdr[0] as string;
                result.Debug = rdr[1] as string;
                result.Info = rdr[2] as string;
                result.Warn = rdr[3] as string;
                result.Error = rdr[4] as string;
                result.Fatal = rdr[5] as string;
                result.StartMessage = rdr[6] as string;
            };

            Action<DatabaseConnection> action = connection => connection.ExecuteReader(onRead, Cmd, beforeRead);
            ExecuteQuery(action);

            return result;
        }

        public void InsertParsingProfile(ParsingTemplate template)
        {
            const string Cmd = @"
                    INSERT INTO ParsingTemplates (
                        Ix, 
                        StartMessage,
                        Trace,
                        Debug,
                        Info,
                        Warn,
                        Error,
                        Fatal
                    )
                    VALUES (
                        @Ix,
                        @StartMessage,
                        @Trace,
                        @Debug,
                        @Info,
                        @Warn,
                        @Error,
                        @Fatal
                    )";
            this.ExecuteNonQuery(Cmd, command => AddParsingTemplateIntoCommand(command, template));
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
            const string ParsingTemplates = @"
                        CREATE TABLE IF NOT EXISTS ParsingTemplates (
                                 Ix INTEGER PRIMARY KEY,
                                 StartMessage TEXT NOT NULL,
                                 Trace TEXT NOT NULL,
                                 Debug TEXT NOT NULL,
                                 Info TEXT NOT NULL,
                                 Warn TEXT NOT NULL,
                                 Error TEXT NOT NULL,
                                 Fatal TEXT NOT NULL
                        );
                    ";
            this.ExecuteNonQuery(stringOptions, integerOptions, booleanOptions, ParsingTemplates);
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
                Debug = GetStringValue(DebugParameterName),
                Trace = GetStringValue(TraceParameterName),
                Info = GetStringValue(InfoParameterName),
                Warn = GetStringValue(WarnParameterName),
                Error = GetStringValue(ErrorParameterName),
                Fatal = GetStringValue(FatalParameterName),
                StartMessage = GetStringValue(StartMessageParameterName)
            };
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

        private static void AddParsingTemplateIntoCommand(IDbCommand command, ParsingTemplate template)
        {
            Action<string, object> action = (name, value) => DatabaseConnection.AddParameter(command, name, value);

            action("@Ix", template.Index);
            action("@StartMessage", template.StartMessage);
            action("@Trace", template.Trace);
            action("@Debug", template.Debug);
            action("@Info", template.Info);
            action("@Warn", template.Warn);
            action("@Error", template.Error);
            action("@Fatal", template.Fatal);
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