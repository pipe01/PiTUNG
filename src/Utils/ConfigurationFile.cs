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
        public int Version { get; set; } = 2;

        public Dictionary<string, object> Entries { get; set; } = new Dictionary<string, object>();

        [DontSerialize]
        public bool AutoSave { get; set; } = true;

        private string FilePath { get; set; }

        private static readonly JSONParameters Parameters = new JSONParameters
        {
            IgnoreAttributes = new List<Type> { typeof(DontSerializeAttribute) }
        };

        public T Get<T>(string key)
        {
            if (Entries.TryGetValue(key, out var val))
            {
                return (T)val;
            }

            throw new KeyNotFoundException();
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (Entries.TryGetValue(key, out var val))
            {
                return (T)val;
            }

            Entries[key] = defaultValue;
            Save();

            return defaultValue;
        }

        public void Set(string key, object value)
        {
            Entries[key] = value;

            if (AutoSave)
                Save();
        }

        public void Save()
        {
            MDebug.WriteLine("SAVE CONFIG TO " + FilePath);
            File.WriteAllText(FilePath, JSON.ToNiceJSON(this, Parameters));
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
                    configFile = JSON.ToObject<ConfigurationFile>(file, Parameters);
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
    }
}
