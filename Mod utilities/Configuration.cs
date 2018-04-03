using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PiTung.Mod_utilities
{
    /// <summary>
    /// Represents a configuration file that is saved on disk for each mod separately.
    /// </summary>
    public static class Configuration
    {
        private static IDictionary<Assembly, ConfigurationFile> Files = new Dictionary<Assembly, ConfigurationFile>();

        /// <summary>
        /// If <see langword="true"/>, the configuration file will be saved to disk every time you call <see cref="Set(string, object)"/>.
        /// </summary>
        public static bool AutoSave
        {
            get => GetConfig(Assembly.GetCallingAssembly()).AutoSave;
            set => GetConfig(Assembly.GetCallingAssembly()).AutoSave = value;
        }

        private static ConfigurationFile GetConfig(Assembly ass)
        {
            if (!Files.TryGetValue(ass, out var cfg))
            {
                var mod = Bootstrapper._Mods.SingleOrDefault(o => o.ModAssembly.FullName.Equals(ass.FullName));

                if (mod == null)
                    mod = Bootstrapper.CurrentlyLoading;

                if (mod == null)
                    return null;

                Files[ass] = new ConfigurationFile(mod);
            }

            return Files[ass];
        }

        /// <summary>
        /// Gets a key on the config and returns it as an object of type <typeparamref name="T"/>. Throws if the key can't be found.
        /// </summary>
        /// <typeparam name="T">The type of the configuration value.</typeparam>
        /// <param name="key">The name of the configuration value.</param>
        /// <returns>The configuration value.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key isn't found in the configuration.</exception>
        public static T Get<T>(string key)
        {
            var cfg = GetConfig(Assembly.GetCallingAssembly());

            return cfg.Get<T>(key);
        }

        /// <summary>
        /// Gets a key on the config and returns it as an object of type <typeparamref name="T"/>. If the key can't be found, the key will be written with value <paramref name="defaultValue"/>.
        /// </summary>
        /// <typeparam name="T">The type of the configuration value.</typeparam>
        /// <param name="key">The name of the configuration value.</param>
        /// <param name="defaultValue">The value that will be saved and returned if <paramref name="key"/> isn't found.</param>
        /// <returns>The configuration value.</returns>
        public static T Get<T>(string key, T defaultValue)
        {
            var cfg = GetConfig(Assembly.GetCallingAssembly());

            return cfg.Get(key, defaultValue);
        }

        /// <summary>
        /// Sets a value on the configuration and saves if <see cref="AutoSave"/> is set to <see langword="true"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set(string key, object value)
        {
            var cfg = GetConfig(Assembly.GetCallingAssembly());

            cfg.Set(key, value);
        }

        public static ConfigurationFile GetConfigFile() => GetConfig(Assembly.GetCallingAssembly());
    }
}
