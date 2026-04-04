using EQX.Core.Common;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;

namespace SDV_MoldingInjection.Services.Security
{
    public interface ISecurityControlStore
    {
        SecurityControlSettings Settings { get; }
        bool ValidatePassword(EPermission permission, string password);
        bool ChangePassword(EPermission permission, string currentPassword, string newPassword);
        void Save();
    }

    public class SecurityControlStore : ISecurityControlStore
    {
        public SecurityControlStore(IConfiguration configuration)
        {
            _configuration = configuration;
            _settingsPath = _configuration["Files:SecurityConfigFile"] ?? @"D:\MoldInjection\Config\SecurityControl.json";

            Settings = Load();
        }

        public SecurityControlSettings Settings { get; private set; }

        public bool ValidatePassword(EPermission permission, string password)
        {
            if (permission == EPermission.Operator && Settings.PasswordMode == EPasswordMode.RMS)
            {
                return true;
            }

            if (permission == EPermission.SuperUser)
            {
                return password == DateTime.Now.ToString("HHdd") || password == GetPassword(permission);
            }

            return password == GetPassword(permission);
        }

        public bool ChangePassword(EPermission permission, string currentPassword, string newPassword)
        {
            if (ValidatePassword(permission, currentPassword) == false)
            {
                return false;
            }

            Settings.Passwords[permission] = newPassword;
            Save();
            return true;
        }

        public void Save()
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (string.IsNullOrWhiteSpace(directory) == false && Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(Settings, JsonSerializerOptions);
            File.WriteAllText(_settingsPath, json);
        }

        private SecurityControlSettings Load()
        {
            if (File.Exists(_settingsPath) == false)
            {
                var defaultSettings = new SecurityControlSettings();
                WriteDefault(defaultSettings);
                return defaultSettings;
            }

            try
            {
                var json = File.ReadAllText(_settingsPath);
                var loaded = JsonSerializer.Deserialize<SecurityControlSettings>(json, JsonSerializerOptions);
                if (loaded == null) return CreateFallbackAndSave();

                EnsureDefaults(loaded);
                return loaded;
            }
            catch
            {
                return CreateFallbackAndSave();
            }
        }

        private SecurityControlSettings CreateFallbackAndSave()
        {
            var fallback = new SecurityControlSettings();
            WriteDefault(fallback);
            return fallback;
        }

        private void WriteDefault(SecurityControlSettings settings)
        {
            Settings = settings;
            Save();
        }

        private void EnsureDefaults(SecurityControlSettings settings)
        {
            foreach (var permission in Enum.GetValues<EPermission>())
            {
                if (settings.Passwords.ContainsKey(permission) == false)
                {
                    settings.Passwords[permission] = permission switch
                    {
                        EPermission.Admin => "1234",
                        EPermission.SuperUser => "3141",
                        _ => string.Empty
                    };
                }
            }

            if (settings.ScreenSaverSec < 10)
            {
                settings.ScreenSaverSec = 10;
            }
        }

        private string GetPassword(EPermission permission)
        {
            if (Settings.Passwords.TryGetValue(permission, out var password) == false)
            {
                return string.Empty;
            }

            return password;
        }

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true
        };

        private readonly IConfiguration _configuration;
        private readonly string _settingsPath;
    }
}
