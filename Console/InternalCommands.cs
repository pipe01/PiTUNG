using PiTung_Bootstrap.Building;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static PiTung_Bootstrap.Console.IGConsole;

namespace PiTung_Bootstrap.Console
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
        public override string Usage => Name;
        public override string Description => "Tries to load new mods";

        public override bool Execute(IEnumerable<string> arguments)
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
        public override string Description => "Prints text to the console.";

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
        public override string Description => "Quits the game.";

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

    internal class Command_add : Command
    {
        public override string Name => "add";
        public override string Usage => Name;

        public override bool Execute(IEnumerable<string> arguments)
        {
            int x = int.Parse(arguments.ElementAt(0));
            int y = int.Parse(arguments.ElementAt(1));
            int id = int.Parse(arguments.ElementAt(2));

            if (BoardManager.Instance.TryGetBoard(id, out var b))
            {
                try
                {
                    Log(b.AddBoardComponent(Components.GetComponent("Inverter"), x, y, 180));
                }
                catch (Exception ex)
                {
                    Error(ex.Message);
                    MDebug.WriteLine(ex);
                }
            }
            else
            {
                Error($"Board with ID {id} not found.");
            }

            return true;
        }
    }

    internal class Command_connect : Command
    {
        public override string Name => "connect";
        public override string Usage => Name;

        public override bool Execute(IEnumerable<string> arguments)
        {
            string io = arguments.ElementAt(0);
            int x1 = int.Parse(arguments.ElementAt(1));
            int y1 = int.Parse(arguments.ElementAt(2));
            int x2 = int.Parse(arguments.ElementAt(3));
            int y2 = int.Parse(arguments.ElementAt(4));
            int id = int.Parse(arguments.ElementAt(5));

            if (BoardManager.Instance.TryGetBoard(id, out var b))
            {
                bool result = false;

                try
                {
                    if (io.Equals("io"))
                    {
                        result = b.ConnectInputOutput(x1, y1, x2, y2);
                    }
                    else if (io.Equals("ii"))
                    {
                        result = b.ConnectInputInput(x1, y1, x2, y2);
                    }
                }
                catch (Exception ex)
                {
                    Error(ex.Message);
                    MDebug.WriteLine(ex);
                }

                Log(result);
            }
            else
            {
                Error($"Board with ID {id} not found.");
            }

            return true;
        }
    }
}
