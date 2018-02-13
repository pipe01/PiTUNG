using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung
{
    public static class ModInput
    {
        private static readonly string BindsPath = Application.persistentDataPath + "/bindings.ini";

        private static Dictionary<string, KeyCode> Binds = new Dictionary<string, KeyCode>();
        
        public static void RegisterBinding(Mod mod, string name, KeyCode defaultKey)
        {
            if (!Binds.ContainsKey(name))
            {
                Binds.Add(name, defaultKey);
                SaveBinds();
            }
        }

        public static KeyCode GetBindingKey(string name)
        {
            if (Binds.TryGetValue(name, out var k))
                return k;

            throw new KeyNotFoundException("Binding not registered.");
        }

        public static bool GetKey(string name) => KeyBool(name, o => Input.GetKey(o));
        public static bool GetKeyDown(string name) => KeyBool(name, o => Input.GetKeyDown(o));
        public static bool GetKeyUp(string name) => KeyBool(name, o => Input.GetKeyUp(o));

        private static bool KeyBool(string name, Func<KeyCode, bool> action)
        {
            if (Binds.TryGetValue(name, out var k))
                return action(k);

            throw new KeyNotFoundException("Binding not registered.");
        }
        
        #region Serialization
        internal static void LoadBinds()
        {
            Binds.Clear();

            if (!File.Exists(BindsPath))
            {
                File.WriteAllText(BindsPath, "");
                return;
            }

            string[] lines = File.ReadAllLines(BindsPath);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.StartsWith(";") || !line.Contains('='))
                    continue;

                int equalsIndex = line.IndexOf('=');
                string key = line.Substring(0, equalsIndex).Trim().Replace(" ", "");
                string value = line.Substring(equalsIndex + 1).Trim();
                
                var keyObj = Enum.Parse(typeof(KeyCode), value, true);

                if (keyObj != null)
                {
                    Binds.Add(key, (KeyCode)keyObj);
                }
            }
        }
        internal static void SaveBinds()
        {
            StringBuilder str = new StringBuilder();

            foreach (var item in Binds.OrderBy(o => o.Key))
            {
                str.AppendLine($"{item.Key} = {Enum.GetName(typeof(KeyCode), item.Value)}");
            }

            File.WriteAllText(BindsPath, str.ToString());
        }
        #endregion
    }
}
