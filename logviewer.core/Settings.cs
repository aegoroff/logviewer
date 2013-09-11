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
        
        private static int GetIntValue(RegistryKey rk, string key)
        {
            var result = (int)rk.GetValue(key);
            if (result == 0)
            {
                RegistryKey.SetValue(key, 0, RegistryValueKind.DWord);
                return 0;
            }
            return result;
        }
        
        private static string GetStringValue(string key)
        {
            return GetStringValue(RegistryKey, key);
        }

        private static void SetStringValue(string key, string value)
        {
            RegistryKey.SetValue(key, value, RegistryValueKind.String);
        }
        
        private static int GetIntValue(string key)
        {
            return GetIntValue(RegistryKey, key);
        }

        private static void SetIntValue(string key, int value)
        {
            RegistryKey.SetValue(key, value, RegistryValueKind.DWord);
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
            get { return GetIntValue(OpenLastFileParameterName) == 1; }
            set { SetIntValue(OpenLastFileParameterName, value ? 1 : 0); }
        }
    }
}