using fastJSON;
using PiTung.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace PiTung.Utils
{
    internal class ConfigurationFile
    {
        public int Version { get; set; } = 1;

        public Dictionary<string, ConfigEntry> Entries { get; set; } = new Dictionary<string, ConfigEntry>();

        [DontSerialize]
        public bool AutoSave { get; set; } = true;

        private string FilePath { get; set; }

        private JSONParameters Parameters = new JSONParameters
        {
            IgnoreAttributes = new List<Type> { typeof(DontSerializeAttribute) }
        };

        public T Get<T>(string key)
        {
            if (Entries.TryGetValue(key, out var val))
            {
                return (T)val.ConvertedValue;
            }

            throw new KeyNotFoundException();
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (Entries.TryGetValue(key, out var val))
            {
                return (T)val.ConvertedValue;
            }

            Entries[key] = new ConfigEntry(defaultValue);
            Save();

            return defaultValue;
        }

        public void Set(string key, object value)
        {
            Entries[key] = new ConfigEntry(value);

            if (AutoSave)
                Save();
        }

        public void Save()
        {
            MDebug.WriteLine("SAVE CONFIG TO " + FilePath);
            File.WriteAllText(FilePath, new JsonFormatter(JSON.ToNiceJSON(this, Parameters)).Format());
        }

        public static ConfigurationFile Load(Mod mod)
        {
            string fileName = mod == null ? "pitung" : mod.PackageName;
            string filePath = Path.Combine(Application.persistentDataPath, Path.Combine("config", fileName + ".json"));

            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            ConfigurationFile configFile;

            if (!File.Exists(filePath))
            {
                configFile = new ConfigurationFile();
                configFile.Save();
            }
            else
            {
                string file = File.ReadAllText(filePath);

                try
                {
                    configFile = JSON.ToObject<ConfigurationFile>(file);
                }
                catch (Exception ex)
                {
                    MDebug.WriteLine($"ERROR: COULDN'T LOAD CONFIGURATION FILE AT '{filePath}'. DETAILS:");
                    MDebug.WriteLine(ex);

                    configFile = new ConfigurationFile();
                }
            }

            configFile.FilePath = filePath;
            return configFile;
        }

        public class ConfigEntry
        {
            public Type Type { get; set; }
            public object Value { get; set; }

            public object ConvertedValue => (Value == null || Type == null) ? null : Convert.ChangeType(Value, Type);

            public ConfigEntry(object value)
            {
                this.Value = value;

                if (value != null)
                    this.Type = value.GetType();
            }
        }
    }
}
