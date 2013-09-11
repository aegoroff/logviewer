using Microsoft.Win32;

namespace logviewer.core
{
    public static class Settings
    {
        /// <summary>
        /// Base name of registry key of application options
        /// </summary>
        internal const string RegistryKeyBase = @"Software\Egoroff\logviewer\";
        private const string OptionsSectionName = @"Options";
        private const string FilterParameterName = @"MessageFilter";
        private const string LastOpenedFileParameterName = @"LastOpenedFile";
        private const string OpenLastFileParameterName = @"OpenLastFile";
        private const string MinLevelParameterName = @"MinLevel";
        private const string MaxLevelParameterName = @"MaxLevel";
        private const string SortingParameterName = @"Sorting";

        private static RegistryKey RegistryKey
        {
            get { return GetRegKey(RegistryKeyBase + OptionsSectionName); }
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
                RegistryKey.SetValue(key, 0, RegistryValueKind.DWord);
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

        public static string MessageFilter
        {
            get { return GetStringValue(FilterParameterName); }
            set { SetStringValue(FilterParameterName, value); }
        }
        
        public static string LastOpenedFile
        {
            get { return GetStringValue(LastOpenedFileParameterName); }
            set { SetStringValue(LastOpenedFileParameterName, value); }
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

        public static bool Sorting
        {
            get { return GetBoolValue(SortingParameterName); }
            set { SetBoolValue(SortingParameterName, value); }
        }
    }
}