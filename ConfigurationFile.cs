using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung
{
    public class ConfigurationFile
    {
        private IDictionary<string, object> InnerDicc;
        private Mod Mod;

        private string FilePath => Path.Combine(Application.persistentDataPath, Path.Combine("config", Mod.PackageName + ".json"));

        internal ConfigurationFile(Mod mod)
        {
            this.Mod = mod;
            Load();
        }

        public T Get<T>(string key)
        {
            if (InnerDicc.TryGetValue(key, out var val))
            {
                return (T)val;
            }

            throw new KeyNotFoundException();
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (InnerDicc.TryGetValue(key, out var val))
            {
                return (T)val;
            }

            InnerDicc[key] = defaultValue;
            Save();

            return defaultValue;
        }

        public void Set(string key, object value)
        {
            InnerDicc[key] = value;
            Save();
        }

        internal void Save()
        {
            MDebug.WriteLine("SAVE CONFIG TO " + FilePath);
            File.WriteAllText(FilePath, SimpleJson.SerializeObject(InnerDicc));
        }

        internal void Load()
        {
            string dir = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(FilePath))
            {
                InnerDicc = new Dictionary<string, object>();
                Save();
            }
            else
            {
                string file = File.ReadAllText(FilePath);

                try
                {
                    InnerDicc = SimpleJson.DeserializeObject<Dictionary<string, object>>(file);
                }
                catch (Exception ex)
                {
                    MDebug.WriteLine($"ERROR: COULDN'T LOAD CONFIGURATION FILE FOR MOD '{Mod.Name}'. DETAILS:");
                    MDebug.WriteLine(ex);

                    InnerDicc = new Dictionary<string, object>();
                }
            }
        }
    }
}
