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
        
        private static string GetStringValue(string key)
        {
            return GetStringValue(RegistryKey, key);
        }

        private static void SetStringValue(string key, string value)
        {
            RegistryKey.SetValue(key, value, RegistryValueKind.String);
        }

        public static string MessageFilter
        {
            get { return GetStringValue(FilterParameterName); }
            set { SetStringValue(FilterParameterName, value); }
        }
    }
}