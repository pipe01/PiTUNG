using PiTung.Console;
using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung
{
    /// <summary>
    /// Manages all mod key bindings. It is highly recommended to use this class instead of <see cref="Input"/>.
    /// </summary>
    public static class ModInput
    {
        private struct KeyBind
        {
            public string ModPackage { get; }
            public string Name { get; }
            public KeyCode Key { get; }

            public KeyBind(string modPackage, string name, KeyCode key)
            {
                this.ModPackage = modPackage;
                this.Name = name;
                this.Key = key;
            }
            public KeyBind(Mod mod, string name, KeyCode key)
            {
                this.ModPackage = mod?.PackageName;
                this.Name = name;
                this.Key = key;
            }
        }

        private static readonly string BindsPath = Application.persistentDataPath + "/bindings.ini";

        private static IList<KeyBind> Binds = new List<KeyBind>();
        
        /// <summary>
        /// Registers a key binding for a mod. If the binding isn't already loaded (it's not set in the file), <paramref name="defaultKey"/> will be the binded key.
        /// </summary>
        /// <param name="mod">The mod that is registering the key.</param>
        /// <param name="name">The binding's name.</param>
        /// <param name="defaultKey">The default binding key.</param>
        /// <exception cref="Exception">Throws if a key binding with name <paramref name="name"/> has already been registered by any other mod.</exception>
        public static void RegisterBinding(Mod mod, string name, KeyCode defaultKey)
        {
            string package = mod?.PackageName ?? "PiTUNG";

            if (Binds.TryGetValue(name, out var k, o => o.Name))
            {
                if (k.ModPackage != package)
                    throw new Exception($"The key binding with name {name} already exists.");
            }
            else
            {
                Binds.Add(new KeyBind(package, name, defaultKey));
                SaveBinds();
            }
        }

        /// <summary>
        /// Gets the <see cref="KeyCode"/> for the binding with name <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the binding we want to get.</param>
        /// <returns>The binding's key code.</returns>
        /// <exception cref="KeyNotFoundException">Throws when a binding isn't found.</exception>
        public static KeyCode GetBindingKey(string name)
        {
            if (Binds.TryGetValue(name, out var k, o => o.Name))
                return k.Key;

            throw new KeyNotFoundException("Binding not registered.");
        }

        public static bool GetKey(string name) => KeyBool(name, o => Input.GetKey(o));
        public static bool GetKeyDown(string name) => KeyBool(name, o => Input.GetKeyDown(o));
        public static bool GetKeyUp(string name) => KeyBool(name, o => Input.GetKeyUp(o));

        private static bool KeyBool(string name, Func<KeyCode, bool> action)
        {
            if (Binds.TryGetValue(name, out var k, o => o.Name))
                return action(k.Key);

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
            string modPack = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.StartsWith("["))
                {
                    modPack = line.Substring(1, line.Length - 2);
                    continue;
                }

                if (line.StartsWith(";") || !line.Contains('='))
                    continue;

                int equalsIndex = line.IndexOf('=');
                string key = line.Substring(0, equalsIndex).Trim().Replace(" ", "");
                string value = line.Substring(equalsIndex + 1).Trim();
                
                var keyObj = Enum.Parse(typeof(KeyCode), value, true);

                if (keyObj != null)
                {
                    Binds.Add(new KeyBind(modPack, key, (KeyCode)keyObj));
                }
            }

            var warnedKeys = new List<KeyCode>();
            foreach (var item in Binds)
            {
                if (Binds.Any(o => o.Key == item.Key && o.Name != item.Name) && !warnedKeys.Contains(item.Key))
                {
                    warnedKeys.Add(item.Key);
                    IGConsole.Error($"The key <b>{item.Key}</b> has been binded to more than once!");
                }
            }
        }
        internal static void SaveBinds()
        {
            int startingIndex = 0;
            StringBuilder str = new StringBuilder();
            string lastMod = null;
            
            //Move all PiTUNG bindings to the top
            while (MoveOne()) ;

            foreach (var item in Binds.OrderBy(o => o.Key))
            {
                if (item.ModPackage != lastMod)
                {
                    if (!Equals(item, Binds[0]))
                        str.AppendLine();

                    str.AppendLine("[" + item.ModPackage + "]");
                    lastMod = item.ModPackage;
                }

                str.AppendLine($"{item.Name} = {Enum.GetName(typeof(KeyCode), item.Key)}");
            }

            File.WriteAllText(BindsPath, str.ToString());

            bool MoveOne()
            {
                for (int i = startingIndex; i < Binds.Count; i++)
                {
                    if (Binds[i].ModPackage.Equals("PiTUNG"))
                    {
                        Binds.Move(i, startingIndex++);
                        return true;
                    }
                }

                return false;
            }
        }
        #endregion
    }
}
