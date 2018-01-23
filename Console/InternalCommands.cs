using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static PiTung_Bootstrap.Console.IGConsole;

namespace PiTung_Bootstrap.Console
{
    internal class Command_help : Command
    {
        public override string Name => "help";
        public override string Usage => $"{Name} [command]";
        public override string Description => "Lists command and shows their usage (help command)";

        public override void Execute(IEnumerable<string> arguments)
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
                    Log(command.Usage);
                }
                else
                {
                    Error($"No such command \"{name}\"");
                }
            }
            else
            {
                Error(Usage);
            }
        }
    }

    internal class Command_lsmod : Command
    {
        public override string Name => "lsmod";
        public override string Usage => $"{Name}";
        public override string Description => "Lists loaded mods (not implemented)";

        public override void Execute(IEnumerable<string> arguments)
        {
            Log("<b>Loaded mods:</b> " + String.Join(", ", Mod.AliveMods.Select(o => $"'{o.Name}'").ToArray()));
        }
    }

    internal class Command_set : Command
    {
        public override string Name => "set";
        public override string Usage => $"{Name} variable [value]";
        public override string Description => "Gets and sets global variables";

        public override void Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 1)
            {
                string variable = arguments.ElementAt(0);
                string value = GetVariable(variable);
                if (value != null)
                    Log(value);
                else
                    Error($"Variable {variable} not set");
            }
            else if (arguments.Count() == 2)
            {
                string variable = arguments.ElementAt(0);
                string value = arguments.ElementAt(1);
                SetVariable(variable, value);
            }
            else
            {
                Error(Usage);
            }
        }
    }
}
