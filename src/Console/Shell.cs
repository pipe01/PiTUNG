using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static PiTung.Console.IGConsole;

namespace PiTung.Console
{
    public static class Shell
    {
        /// <summary>
        /// Command registry (name -> Command)
        /// </summary>
        internal static Dictionary<string, Command> Registry;

        /// <summary>
        /// Variable registry (name -> value)
        /// </summary>
        private static Dictionary<string, string> VarRegistry;

        static Shell()
        {
            Registry = new Dictionary<string, Command>();
            VarRegistry = new Dictionary<string, string>();
        }
        
        internal static void LoadCommands()
        {
            foreach (var item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (item.Name.StartsWith("Command_", StringComparison.InvariantCulture) && item.BaseType == typeof(Command))
                {
                    Shell.RegisterCommand((Command)Activator.CreateInstance(item));
                }
            }
        }

        /// <summary>
        /// Saves the value of a global variable
        /// </summary>
        /// <param name="variable">The variable to set</param>
        /// <param name="value">The value to give</param>
        public static void SetVariable(string variable, string value)
        {
            VarRegistry[variable] = value;
        }

        /// <summary>
        /// Obtains the value of a global variable
        /// </summary>
        /// <param name="variable">The variable to get</param>
        /// <returns>The value, or null if variable is not set</returns>
        public static string GetVariable(string variable)
        {
            string value;
            if (VarRegistry.TryGetValue(variable, out value))
                return value;
            return null;
        }

        /// <summary>
        /// Returns the names of the variables in the registry
        /// </summary>
        /// <returns>An enumerable containing the names of the variables</returns>
        public static IEnumerable<string> GetVariables()
        {
            return VarRegistry.Keys;
        }

        /// <summary>
        /// Returns the names of the available commands
        /// </summary>
        /// <returns>An enumerable containing the names of the variables</returns>
        public static IEnumerable<string> GetCommandNames()
        {
            return Registry.Keys;
        }


        internal static bool RegisterCommandInner(Command command, Mod mod)
        {
            if (command.Mod == null)
                command.Mod = mod;

            if (Registry.ContainsKey(command.Name))
                return false;

            Registry.Add(command.Name, command);
            return true;
        }

        /// <summary>
        /// Register a command.
        /// </summary>
        /// <param name="command">The command class.</param>
        public static bool RegisterCommand(Command command)
        {
            return RegisterCommandInner(command, Bootstrapper.Instance.GetModByAssembly(Assembly.GetCallingAssembly(), false));
        }

        /// <summary>
        /// Register a command.
        /// </summary>
        /// <typeparam name="T">The type of the command.</typeparam>
        public static bool RegisterCommand<T>() where T : Command
        {
            return RegisterCommandInner(Activator.CreateInstance<T>(), Bootstrapper.Instance.GetModByAssembly(Assembly.GetCallingAssembly(), false));
        }
        
        /// <summary>
        /// Removes a command from the registry
        /// </summary>
        /// <param name="name">Name of the command to remove</param>
        /// <returns>True if a command was removed, false otherwise</returns>
        internal static bool UnregisterCommand(string name)
        {
            if (Registry.ContainsKey(name))
            {
                Registry.Remove(name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called when the user presses enter
        /// </summary>
        /// <param name="cmd">The full command line</param>
        internal static void ExecuteCommand(string cmd)
        {
            if (cmd.Length == 0)
                return;

            string verb, error;
            string[] args;

            if (!CmdParser.TryParseCmdLine(cmd, out verb, out args, out error))
            {
                Log(LogType.ERROR, "Invalid command: " + error);
                return;
            }

            args = ReplaceVariables(args).ToArray();

            Command command;

            if (Registry.TryGetValue(verb, out command))
            {
                try
                {
                    command.Execute(args);
                }
                catch (Exception e)
                {
                    Log(LogType.ERROR, "An internal error occurred while executing the command.");
                    MDebug.WriteLine("Command exception: '" + cmd + "'");
                    MDebug.WriteLine(e);
                }
            }
            else if (!TryParseVariables(cmd.Trim()))
            {
                Log(LogType.ERROR, $"Unrecognized command: {verb}");
            }
        }

        /// <summary>
        /// Tries to parse variable setting and getting.
        /// </summary>
        /// <param name="line">The command line.</param>
        /// <returns>True if a variable was parsed.</returns>
        private static bool TryParseVariables(string line)
        {
            if (line.StartsWith("$"))
            {
                if (line.Contains('='))
                {
                    int equalsIndex = line.IndexOf('=');
                    string name = line.Substring(1, equalsIndex - 1).Trim();
                    string val = line.Substring(equalsIndex + 1).Trim();

                    if (val.StartsWith("\"") && val.EndsWith("\""))
                    {
                        val = val.Substring(1, val.Length - 2).Trim();
                    }

                    SetVariable(name, val);
                    Log($"\"{val}\"");

                    return true;
                }

                string variable = line.Substring(1);

                if (variable.Contains(' '))
                    return false;

                string value = GetVariable(variable);

                if (value != null)
                {
                    Log($"\"{value}\"");
                }
                else
                {
                    Log(LogType.ERROR, $"Variable \"{variable}\" not found.");
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Replaces all $variables with their corresponding values.
        /// </summary>
        /// <param name="arguments">The current command arguments.</param>
        private static IEnumerable<string> ReplaceVariables(string[] arguments)
        {
            foreach (var item in arguments.Select(o => o.Trim()))
            {
                string ret = "";

                for (int i = 0; i < item.Length; i++)
                {
                    char c = item[i];

                    if (c == '$' && (i == 0 || item[i - 1] != '\\'))
                    {
                        string varName = ReadVarName(item.Substring(i + 1));
                        string varValue = GetVariable(varName);

                        if (varValue != null)
                        {
                            ret += varValue;
                        }
                    }
                }

                if (ret?.Length == 0)
                    ret = item;

                yield return ret;
            }

            string ReadVarName(string arg)
            {
                string str = "";

                for (int i = 0; i < arg.Length; i++)
                {
                    char c = arg[i];

                    if (c == '$' && (i == 0 || arg[i - 1] != '\\'))
                    {
                        break;
                    }
                    else
                    {
                        str += c;
                    }
                }

                return str;
            }
        }


        /// <summary>
        /// Returns the auto-completion candidates for a given command.
        /// Depending on what is entered, it will either return command verbs
        /// or command-provided auto-completion candidates
        /// </summary>
        /// <param name="command">The current command [verb, arg1, arg2,...]</param>
        /// <returns>The auto-completion candidates for the last argument</returns>
        internal static IEnumerable<String> GetAutocompletionCandidates(IEnumerable<String> command)
        {
            if (!command.Any())
                return new List<String>();

            String verb = command.ElementAt(0);
            if (command.Count() == 1)
                return Autocompletion.Candidates(verb, Registry.Keys);

            Command _command;
            if (Registry.TryGetValue(verb, out _command))
                return _command.AutocompletionCandidates(command.Skip(1));

            return new List<String>();
        }

    }
}
