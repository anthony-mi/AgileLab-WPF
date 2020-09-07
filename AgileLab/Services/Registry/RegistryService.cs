using AgileLab.Services.Logging;
using Microsoft.Win32;
using System;

namespace AgileLab.Services.Registry
{
    class RegistryService : IRegistryService
    {
        private const string REGISTRY_DIRECTORY = "SOFTWARE\\AgileLab";

        private const string PASSWORD_HASH_KEY = "PasswordHash";
        private const string USERNAME_KEY = "Username";
        private const string CURRENT_TEAM_KEY = "Team";
        private const string CURRENT_PROJECT_KEY = "Project";

        public void SetPasswordHash(string hash) => TrySetStringKey(PASSWORD_HASH_KEY, hash);
        public void SetUsername(string username) => TrySetStringKey(USERNAME_KEY, username);
        public void SetCurrentTeamId(uint id) => TrySetQWordKey(CURRENT_TEAM_KEY, id);
        public void SetCurrentProjectId(uint id) => TrySetQWordKey(CURRENT_PROJECT_KEY, id);

        public string GetPasswordHash() => TryGetStringKey(PASSWORD_HASH_KEY);
        public string GetUsername() => TryGetStringKey(USERNAME_KEY);
        public uint GetCurrentTeamId() => Convert.ToUInt32(TryGetQWordKey(CURRENT_TEAM_KEY));
        public uint GetCurrentProjectId() => Convert.ToUInt32(TryGetQWordKey(CURRENT_PROJECT_KEY));

        private static ILogger _logger = ComponentsContainer.Get<ILogger>();

        private static void TrySetStringKey(string key, string value)
        {
            try
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(REGISTRY_DIRECTORY);

                RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REGISTRY_DIRECTORY, true);

                registryKey?.SetValue(key, value, RegistryValueKind.String);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed setting registry value for key {key}");
                _logger.Warning(ex);
            }
        }

        private static void TrySetQWordKey(string key, uint value)
        {
            try
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(REGISTRY_DIRECTORY);

                RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REGISTRY_DIRECTORY, true);

                registryKey?.SetValue(key, value, RegistryValueKind.QWord);
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed setting registry value for key {key}");
                _logger.Warning(ex);
            }
        }

        private static string TryGetStringKey(string key)
        {
            try
            {
                RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REGISTRY_DIRECTORY, true);

                if (registryKey == null)
                    return null;

                string value = registryKey.GetValue(key) as string;

                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed getting registry value for key {key}");
                _logger.Warning(ex);
            }

            return null;
        }

        private static Int64 TryGetQWordKey(string key)
        {
            try
            {
                RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REGISTRY_DIRECTORY, true);

                if (registryKey == null)
                {
                    return 0;
                }

                long value = Convert.ToInt64(registryKey.GetValue(key));

                return value;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed getting registry value for key {key}");
                _logger.Warning(ex);
            }

            return 0;
        }

        public void RemoveAllData()
        {
            try
            {
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(REGISTRY_DIRECTORY);
            }
            catch
            {
                SetPasswordHash(string.Empty);
                SetUsername(string.Empty);
                SetCurrentTeamId(0);
                SetCurrentProjectId(0);
            }
        }
    }
}
