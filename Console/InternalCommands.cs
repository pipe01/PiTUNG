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
                foreach (Command command in Registry.Values)
                {
                    string log = $"<b>{command.Name}</b>";
                    if (command.Description != null)
                        log += ": " + command.Description;
                    Log(log);
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
        }
    }

    internal class Command_lsmod : Command
    {
        public override string Name => "lsmod";
        public override string Usage => Name;
        public override string Description => "Lists loaded mods";

        public override bool Execute(IEnumerable<string> arguments)
        {
            Log("<b>Loaded mods:</b> " + String.Join(", ", Mod.AliveMods.Select(o => $"'{o.Name}'").ToArray()));

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
}
