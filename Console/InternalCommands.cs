using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static PiTung.Console.IGConsole;

namespace PiTung.Console
{
    internal class Command_help : Command
    {
        public override string Name => "help";
        public override string Usage => $"{Name} [command]";
        public override string Description => "Lists command and shows their usage (help command)";

        public override bool Execute(IEnumerable<string> arguments)
        {
            if (!arguments.Any())
            {
                var cmds = from o in Registry.Values
                           where o.ShowOnHelp
                           orderby o.Name
                           select o;

                Log("<color=orange>PiTUNG</color>:");
                foreach (var cmd in cmds.Where(o => o.Mod == null))
                {
                    Log(GetCommandLine(cmd));
                }

                Mod prevMod = null;
                foreach (var cmd in cmds.Where(o => o.Mod != null).OrderBy(o => o.Mod.Name))
                {
                    if (prevMod != cmd.Mod)
                    {
                        prevMod = cmd.Mod;
                        Log($"\n<color=orange>{cmd.Mod.Name}</color>:");
                    }

                    Log(GetCommandLine(cmd));
                }
            }
            else if (arguments.Count() == 1)
            {
                string name = arguments.ElementAt(0);
                Command command;
                if (Registry.TryGetValue(name, out command))
                {
                    Log(command.Description);
                    Log("<b>Usage:</b> " + command.Usage);
                }
                else
                {
                    Error($"No such command \"{name}\"");
                }
            }
            else
            {
                return false;
            }

            return true;

            string GetCommandLine(Command command)
            {
                string log = $"<b>{command.Name}</b>";
                if (command.Description != null)
                    log += ": " + command.Description;
                return log;
            }
        }
    }

    internal class Command_mods : Command
    {
        public override string Name => "mods";
        public override string Usage => Name;
        public override string Description => "Lists loaded mods";

        public override bool Execute(IEnumerable<string> arguments)
        {
            Log($"<b>Loaded mods ({Bootstrapper.ModCount}):</b> " + String.Join(", ", Mod.AliveMods.Select(o => $"'{o.FullName}'").ToArray()));
            
            return true;
        }
    }

    internal class Command_set : Command
    {
        public override string Name => "set";
        public override string Usage => $"{Name} variable [value]";
        public override string Description => "Sets global variables";

        public override bool Execute(IEnumerable<string> arguments)
        {
            string value = null;
            string variable = arguments.FirstOrDefault();

            if (arguments.Count() == 1)
            {
                //Clear variable
                value = "";
            }
            else if (arguments.Count() == 2)
            {
                value = arguments.ElementAt(1);
            }
            else
            {
                return false;
            }

            SetVariable(variable, value);
            return true;
        }
    }

    internal class Command_get : Command
    {
        public override string Name => "get";
        public override string Usage => $"{Name} variable";
        public override string Description => "Gets global variables";

        public override bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 1)
            {
                string variable = arguments.First();
                string value = GetVariable(variable);

                if (value != null)
                {
                    Log($"\"{variable}\" = \"{value}\"");
                }
                else
                {
                    Log(LogType.ERROR, $"Variable \"{variable}\" not found.");
                }

                return true;
            }

            return false;
        }
    }

    internal class Command_reload : Command
    {
        public override string Name => "reload";
        public override string Usage => $"{Name} [mod name]";
        public override string Description => "Loads new mods or reloads an existing one";

        public override bool Execute(IEnumerable<string> arguments)
        {
            int count = arguments.Count();

            if (count == 1)
            {
                return ReloadMod(arguments.First());
            }
            else if (count == 0)
            {
                return LoadMods();
            }
            else
            {
                return false;
            }
        }

        private bool ReloadMod(string name)
        {
            var mod = Mod.AliveMods.SingleOrDefault(o => o.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (mod == null)
            {
                Error($"Can't find any mod with name '{name}'.");
                return true;
            }

            name = mod.Name;

            if (!mod.Reloadable)
            {
                Error($"Mod '{name}' isn't reloadable.");
                return true;
            }

            int index = Bootstrapper._Mods.IndexOf(mod);
            Bootstrapper._Mods.Remove(mod);

            try
            {
                mod = ModLoader.GetMod(mod.FullPath);
            }
            catch
            {
                mod = null;
            }

            if (mod == null)
            {
                Error($"An error occurred while loading mod '{name}'.");
            }
            else
            {
                Bootstrapper.Instance.LoadMod(mod, false);
            }

            return true;
        }

        private bool LoadMods()
        {
            int oldCount = Bootstrapper.ModCount;

            Bootstrapper.Instance.Patch(true);

            int newMods = Bootstrapper.ModCount - oldCount;

            Log($"Done. {newMods} new mods loaded.");

            return true;
        }
    }

    internal class Command_echo : Command
    {
        public override string Name => "echo";
        public override string Usage => $"{Name} text";
        public override string Description => "Prints text to the console";

        public override bool Execute(IEnumerable<string> arguments)
        {
            string str = string.Join(" ", arguments.ToArray());

            Log(str);

            return true;
        }
    }

    internal class Command_quit : Command
    {
        public override string Name => "quit";
        public override string Usage => Name;
        public override string Description => "Quits the game";

        public override bool Execute(IEnumerable<string> arguments)
        {
            Application.Quit();
            return true;
        }
    }

    internal class Command_objs : Command
    {
        public override string Name => "objs";
        public override string Usage => Name;
        internal override bool ShowOnHelp => false;

        public override bool Execute(IEnumerable<string> arguments)
        {
            var scene = SceneManager.GetActiveScene();

            foreach (var obj in scene.GetRootGameObjects())
            {
                Recurse(obj);
            }

            return true;

            void Recurse(GameObject parent, int level = 0)
            {
                string compStr = string.Join(", ", parent.GetComponents<Component>().Select(o => o.GetType().Name).ToArray());
                
                MDebug.WriteLine(new string(' ', level * 4) + parent.name + $" ({compStr})");

                foreach (Transform item in parent.transform)
                {
                    Recurse(item.gameObject, level + 1);
                }
            }
        }
    }

    internal class Command_praise : Command
    {
        public override string Name => "praise";
        public override string Usage => "you know what to do";
        internal override bool ShowOnHelp => false;

        public override bool Execute(IEnumerable<string> arguments)
        {
            var args = arguments.ToArray();

            if (args.Length >= 2 && args[0] == "the" && args[1] == "sun")
            {
                Log("May your life be full of joy and happiness.");
                BoringStuff.SUMMONEXODIA();
            }
            else
            {
                Error("HERESY WILL NOT BE TOLERATED");
            }

            return true;
        }
    }

    internal class Command_bind : Command
    {
        public override string Name => "bind";
        public override string Usage => $"{Name} <bind name> [key]";
        public override string Description => "Gets or sets a key binding";

        public override bool Execute(IEnumerable<string> arguments)
        {
            int count = arguments.Count();

            if (count == 1)
            {
                return PrintBinding(arguments.First());
            }
            else if (count == 2)
            {
                return SetBinding(arguments.First(), arguments.ElementAt(1));
            }
            else
            {
                return false;
            }
        }

        private ModInput.KeyBind GetBinding(string bindName)
        {
            var bind = ModInput.Binds.SingleOrDefault(o => o.Name.Equals(bindName, StringComparison.InvariantCultureIgnoreCase));

            if (EqualityComparer<ModInput.KeyBind>.Default.Equals(bind, default(ModInput.KeyBind)))
            {
                Error($"Binding '{bindName}' not found.");
                return null;
            }

            return bind;
        }

        private bool PrintBinding(string name)
        {
            var bind = GetBinding(name);

            if (bind == null)
                return true;

            Log($"<color=lime>Binding <b>{bind.Name}</b></color> <color=orange>=</color> {bind.Key}");

            return true;
        }

        private bool SetBinding(string name, string keyStr)
        {
            KeyCode key = KeyCode.None;

            foreach (var item in Enum.GetNames(typeof(KeyCode)))
            {
                if (item.Equals(keyStr, StringComparison.InvariantCultureIgnoreCase))
                {
                    key = (KeyCode)Enum.Parse(typeof(KeyCode), keyStr, true);
                    break;
                }
            }

            if (key == KeyCode.None)
            {
                Error($"Invalid key '{keyStr}'.");
                return true;
            }

            var b = GetBinding(name);

            if (b == null)
                return true;

            b.Key = key;
            ModInput.SaveBinds();

            PrintBinding(name);

            return true;
        }
    }
}
