using PiTung.Console;
using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace PiTung
{
    /// <summary>
    /// Contains the modifier keys that may be down when a certain binding key is pressed.
    /// </summary>
    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Control = 1,
        Shift = 2,
        Alt = 4
    }

    /// <summary>
    /// Manages all mod key bindings. It is highly recommended to use this class instead of <see cref="Input"/>.
    /// </summary>
    public static class ModInput
    {
        internal class KeyBind
        {
            public string ModPackage { get; }
            public string Name { get; }
            public KeyCode Key { get; internal set; }
            public RegisterActions Listener { get; }
            public KeyModifiers Modifiers { get; }

            public KeyBind(string modPackage, string name, KeyCode key, RegisterActions l, KeyModifiers mods)
            {
                this.ModPackage = modPackage;
                this.Name = name;
                this.Key = key;
                this.Listener = l;
                this.Modifiers = mods;
            }
        }

        public class RegisterActions
        {
            internal Action OnKey, OnKeyDown, OnKeyUp;

            internal RegisterActions()
            {
            }

            public RegisterActions ListenKey(Action action)
            {
                this.OnKey = action;
                return this;
            }

            public RegisterActions ListenKeyDown(Action action)
            {
                this.OnKeyDown = action;
                return this;
            }

            public RegisterActions ListenKeyUp(Action action)
            {
                this.OnKeyUp = action;
                return this;
            }
        }

        private static readonly string BindsPath = Application.persistentDataPath + "/bindings.ini";

        internal static IList<KeyBind> Binds = new List<KeyBind>();
        
        internal static bool CheckingInput { get; private set; }

        /// <summary>
        /// Registers a key binding for a mod. If the binding isn't already loaded (it's not set in the file), <paramref name="defaultKey"/> will be the binded key.
        /// </summary>
        /// <param name="mod">The mod that is registering the key. This parameter can be omitted (see <see cref="RegisterBinding(string, KeyCode, KeyModifiers)"/>).</param>
        /// <param name="name">The binding's name.</param>
        /// <param name="defaultKey">The default binding key.</param>
        /// <exception cref="Exception">Throws if a key binding with name <paramref name="name"/> has already been registered by any other mod.</exception>
        public static RegisterActions RegisterBinding(Mod mod, string name, KeyCode defaultKey, KeyModifiers modifiers = KeyModifiers.None)
        {
            string package = mod?.PackageName ?? "PiTUNG";

            if (Binds.TryGetValue(name, out var k, o => o.Name))
            {
                if (k.ModPackage != package)
                    throw new Exception($"The key binding with name {name} already exists.");
                else
                    return k.Listener;
            }
            else
            {
                var listener = new RegisterActions();

                Binds.Add(new KeyBind(package, name, defaultKey, listener, modifiers));
                SaveBinds();

                return listener;
            }
        }

        /// <summary>
        /// Registers a key binding for a mod. If the binding isn't already loaded (it's not set in the file), <paramref name="defaultKey"/> will be the binded key.
        /// </summary>
        /// <param name="name">The binding's name.</param>
        /// <param name="defaultKey">The default binding key.</param>
        /// <exception cref="Exception">Throws if a key binding with name <paramref name="name"/> has already been registered by any other mod.</exception>
        public static RegisterActions RegisterBinding(string name, KeyCode defaultKey, KeyModifiers modifiers = KeyModifiers.None)
        {
            var ass = Assembly.GetCallingAssembly();
            var mod = Bootstrapper.Instance.GetModByAssembly(ass);

            return RegisterBinding(mod, name, defaultKey, modifiers);
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

        internal static void UpdateListeners()
        {
            foreach (var item in Binds)
            {
                if (item.ModPackage != "PiTUNG" && IGConsole.Shown)
                    continue;

                if (item.Listener != null && CheckModifiers(item))
                {
                    var l = item.Listener;

                    if (l.OnKey != null && GetKey(item.Name))
                        l.OnKey();

                    if (l.OnKeyDown != null && GetKeyDown(item.Name))
                        l.OnKeyDown();

                    if (l.OnKeyUp != null && GetKeyUp(item.Name))
                        l.OnKeyUp();
                }
            }
        }

        private static bool CheckModifiers(KeyBind bind)
        {
            bool hasCtrl = HasMod(bind.Modifiers, KeyModifiers.Control);
            bool hasShift = HasMod(bind.Modifiers, KeyModifiers.Shift);
            bool hasAlt = HasMod(bind.Modifiers, KeyModifiers.Alt);

            int targetFlags = (hasCtrl ? (1 << 0) : 0) | (hasShift ? (1 << 1) : 0) | (hasAlt ? (1 << 2) : 0);
            int inputFlags =
                (Input.GetKey(KeyCode.LeftControl) ? (1 << 0) : 0) |
                (Input.GetKey(KeyCode.LeftShift) ? (1 << 1) : 0) |
                (Input.GetKey(KeyCode.LeftAlt) ? (1 << 2) : 0);
            
            return targetFlags == inputFlags;

            bool HasMod(KeyModifiers mods, KeyModifiers mod)
                => (mods & mod) == mod;
        }
        
        private static bool KeyBool(string name, Func<KeyCode, bool> action)
        {
            if (Binds.TryGetValue(name, out var bind, o => o.Name))
            {
                return action(bind.Key) && CheckModifiers(bind);
            }

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

                var mods = ParseModifiers(value, out string litKey);
                var keyObj = Enum.Parse(typeof(KeyCode), litKey, true);

                if (keyObj != null)
                {
                    Binds.Add(new KeyBind(modPack, key, (KeyCode)keyObj, new RegisterActions(), mods));
                }
            }

            var warnedKeys = new List<KeyCode>();
            foreach (var item in Binds)
            {
                if (Binds.Any(o => o.Key == item.Key && o.Modifiers == item.Modifiers && o.Name != item.Name) && !warnedKeys.Contains(item.Key))
                {
                    warnedKeys.Add(item.Key);
                    IGConsole.Error($"The key <b>{AddModifiers(item.Key.ToString(), item.Modifiers)}</b> has been binded to more than once!");
                }
            }
        }
        internal static void SaveBinds()
        {
            int startingIndex = 0;
            var str = new StringBuilder();
            string lastMod = null;

            var bindsCopy = new List<KeyBind>(Binds)
                .OrderBy(o => o.ModPackage)
                .ThenBy(o => o.Name)
                .ToList();

            //Move all PiTUNG bindings to the top
            while (MoveOne(bindsCopy)) ;
            
            foreach (var item in bindsCopy)
            {
                if (item.ModPackage != lastMod)
                {
                    if (!Equals(item, Binds[0]))
                        str.AppendLine();

                    str.AppendLine("[" + item.ModPackage + "]");
                    lastMod = item.ModPackage;
                }

                string keyName = Enum.GetName(typeof(KeyCode), item.Key);
                string value = AddModifiers(keyName, item.Modifiers);

                str.AppendLine($"{item.Name} = {value}");
            }

            File.WriteAllText(BindsPath, str.ToString());

            bool MoveOne(IList<KeyBind> list)
            {
                for (int i = startingIndex; i < list.Count; i++)
                {
                    if (list[i].ModPackage.Equals("PiTUNG"))
                    {
                        list.Move(i, startingIndex++);
                        return true;
                    }
                }

                return false;
            }
        }

        internal static string AddModifiers(string key, KeyModifiers mods)
        {
            string ret = "";

            if ((mods & KeyModifiers.Control) == KeyModifiers.Control)
                ret += "+";

            if ((mods & KeyModifiers.Shift) == KeyModifiers.Shift)
                ret += "-";

            if ((mods & KeyModifiers.Alt) == KeyModifiers.Alt)
                ret += "!";

            return ret + key;
        }

        internal static KeyModifiers ParseModifiers(string value, out string literalKey)
        {
            var ret = KeyModifiers.None;
            int i;

            for (i = 0; i < value.Length; i++)
            {
                bool exit = false;

                switch (value[i])
                {
                    case '+':
                        ret |= KeyModifiers.Control;
                        break;
                    case '-':
                        ret |= KeyModifiers.Shift;
                        break;
                    case '!':
                        ret |= KeyModifiers.Alt;
                        break;
                    default:
                        exit = true;
                        break;
                }

                if (exit)
                    break;
            }

            literalKey = value.Substring(i);
            return ret;
        }
        #endregion
    }
}