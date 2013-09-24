// Created by: egr
// Created at: 02.05.2013
// © 2012-2013 Alexander Egorov

using System;
using System.Data;
using System.IO;
using Microsoft.Win32;

namespace logviewer.core
{
    public class Settings
    {
        private readonly string settingsDatabaseFilePath;

        /// <summary>
        /// Base name of registry key of application options
        /// </summary>
        internal const string RegistryKeyBase = @"Software\Egoroff\logviewer\";

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

        private static RegistryKey RegistryKey
        {
            get { return GetRegKey(RegistryKeyBase + OptionsSectionName); }
        }

        public Settings(string settingsDatabaseFileName)
        {
            this.settingsDatabaseFilePath = Path.Combine(ApplicationFolder, settingsDatabaseFileName);
            this.CreateTables();
        }

        private void CreateTables()
        {
            const string StringOptions = @"
                        CREATE TABLE IF NOT EXISTS StringOptions (
                                 Option TEXT PRIMARY KEY,
                                 Value TEXT
                        );
                    ";
            const string IntegerOptions = @"
                        CREATE TABLE IF NOT EXISTS IntegerOptions (
                                 Option TEXT PRIMARY KEY,
                                 Value INTEGER
                        );
                    ";
            const string BooleanOptions = @"
                        CREATE TABLE IF NOT EXISTS BooleanOptions (
                                 Option TEXT PRIMARY KEY,
                                 Value BOOLEAN
                        );
                    ";
            this.ExecuteNonQuery(StringOptions, IntegerOptions, BooleanOptions);
        }

        private void ExecuteNonQuery(params string[] queries)
        {
            var connection = new DatabaseConnection(this.settingsDatabaseFilePath);
            using (connection)
            {
                connection.RunSqlQuery(command => command.ExecuteNonQuery(), queries);
            }
        }

        private void RunSqlQuery(Action<IDbCommand> action, params string[] commands)
        {
            var connection = new DatabaseConnection(this.settingsDatabaseFilePath);
            using (connection)
            {
                connection.RunSqlQuery(action, commands);
            }
        }

        /// <summary>
        /// Open/create regkey for keeping data
        /// </summary>
        /// <param name="key">key name</param>
        /// <returns>Exist or new RegistryKey object</returns>
        private static RegistryKey GetRegKey(string key)
        {
            return Registry.CurrentUser.OpenSubKey(key, true) ?? Registry.CurrentUser.CreateSubKey(key);
        }

        private static string GetStringValue(RegistryKey rk, string key)
        {
            var result = rk.GetValue(key) as string;
            if (result == null)
            {
                RegistryKey.SetValue(key, string.Empty, RegistryValueKind.String);
                return string.Empty;
            }
            return result;
        }

        private static int GetIntValue(RegistryKey rk, string key, int defaultValue = default(int))
        {
            var obj = rk.GetValue(key);
            if (obj == null)
            {
                RegistryKey.SetValue(key, defaultValue, RegistryValueKind.DWord);
                return defaultValue;
            }
            return (int)obj;
        }

        private static string GetStringValue(string key)
        {
            return GetStringValue(RegistryKey, key);
        }

        private static void SetStringValue(string key, string value)
        {
            RegistryKey.SetValue(key, value, RegistryValueKind.String);
        }

        private static int GetIntValue(string key, int defaultValue = default(int))
        {
            return GetIntValue(RegistryKey, key, defaultValue);
        }

        private static bool GetBoolValue(string key)
        {
            return GetIntValue(key) == 1;
        }

        private static void SetIntValue(string key, int value)
        {
            RegistryKey.SetValue(key, value, RegistryValueKind.DWord);
        }

        private static void SetBoolValue(string key, bool value)
        {
            SetIntValue(key, value ? 1 : 0);
        }

        private const string ApplicationOptionsFolder = "logviewer";
        private static readonly string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static readonly string applicationFolder = Path.Combine(baseFolder, ApplicationOptionsFolder);

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

        public static string MessageFilter
        {
            get { return GetStringValue(FilterParameterName); }
            set { SetStringValue(FilterParameterName, value); }
        }

        public static bool OpenLastFile
        {
            get { return GetBoolValue(OpenLastFileParameterName); }
            set { SetBoolValue(OpenLastFileParameterName, value); }
        }

        public static int MinLevel
        {
            get { return GetIntValue(MinLevelParameterName); }
            set { SetIntValue(MinLevelParameterName, value); }
        }

        public static int MaxLevel
        {
            get { return GetIntValue(MaxLevelParameterName, (int)LogLevel.Fatal); }
            set { SetIntValue(MaxLevelParameterName, value); }
        }

        public static int PageSize
        {
            get { return GetIntValue(PageSizeParameterName, 5000); }
            set { SetIntValue(PageSizeParameterName, value); }
        }

        public static bool Sorting
        {
            get { return GetBoolValue(SortingParameterName); }
            set { SetBoolValue(SortingParameterName, value); }
        }

        public static bool UseRegexp
        {
            get { return GetBoolValue(UseRegexpParameterName); }
            set { SetBoolValue(UseRegexpParameterName, value); }
        }

        public static string StartMessageTemplate
        {
            get { return GetStringValue(StartMessageParameterName); }
            set { SetStringValue(StartMessageParameterName, value); }
        }

        public static string TraceLevel
        {
            get { return GetStringValue(TraceParameterName); }
            set { SetStringValue(TraceParameterName, value); }
        }

        public static string DebugLevel
        {
            get { return GetStringValue(DebugParameterName); }
            set { SetStringValue(DebugParameterName, value); }
        }

        public static string InfoLevel
        {
            get { return GetStringValue(InfoParameterName); }
            set { SetStringValue(InfoParameterName, value); }
        }

        public static string WarnLevel
        {
            get { return GetStringValue(WarnParameterName); }
            set { SetStringValue(WarnParameterName, value); }
        }

        public static string ErrorLevel
        {
            get { return GetStringValue(ErrorParameterName); }
            set { SetStringValue(ErrorParameterName, value); }
        }

        public static string FatalLevel
        {
            get { return GetStringValue(FatalParameterName); }
            set { SetStringValue(FatalParameterName, value); }
        }

        public static int KeepLastNFiles
        {
            get { return GetIntValue(KeepLastNFilesParameterName, 10); }
            set { SetIntValue(KeepLastNFilesParameterName, value); }
        }

        public static Func<string>[] LevelReaders
        {
            get
            {
                return new Func<string>[]
                {
                    () => TraceLevel,
                    () => DebugLevel,
                    () => InfoLevel,
                    () => WarnLevel,
                    () => ErrorLevel,
                    () => FatalLevel
                };
            }
        }

        public string SettingsDatabaseFilePath
        {
            get { return this.settingsDatabaseFilePath; }
        }
    }
}