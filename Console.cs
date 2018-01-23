using PiTung_Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polyglot
{
    /// <summary>
    /// Type of a log (should be self-explanatory)
    /// </summary>
    public enum LogType
    {
        INFO,
        USERINPUT,
        ERROR
    }

    /// <summary>
    /// A line of log. It has a message and a log type
    /// </summary>
    internal class LogEntry
    {
        public LogType Type { get; private set; }
        public string Message { get; private set; }

        public LogEntry(LogType type, string message)
        {
            this.Type = type;
            this.Message = message;
        }

        public Color GetColor()
        {
            switch(Type)
            {
                case LogType.INFO:
                    return Color.white;
                case LogType.USERINPUT:
                    return Color.cyan;
                case LogType.ERROR:
                    return Color.red;
            }
            return Color.white;
        }
    }

    /// <summary>
    /// Represents a command that can be invoked from the console
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Used to invoke the command (e.g. "help")
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// How to use the command (e.g. $"{Name} argument [optional_argument]")
        /// </summary>
        public abstract string Usage { get; }

        /// <summary>
        /// Short description of what the command does, preferably on 1 line
        /// </summary>
        public virtual string Description { get; } = null;

        /// <summary>
        /// Called when the command is invoked
        /// </summary>
        /// <param name="arguments">The arguments given to the command</param>
        public abstract void Execute(IEnumerable<string> arguments);
    }

    //FIXME: Changing scene to gameplay locks mouse
    //TODO: Add verbosity levels
    //TODO: Parse string literals as one argument

    /// <summary>
    /// In game console
    /// <para>This static class allows for logging and registering commands
    /// which will be executed by callbacks. Contains a dictionary of
    /// global variables that can be read and written to from the console
    /// using the "set" command</para>
    /// </summary>
    public class Console
    {
        /// <summary>
        /// Max number of log entries kept in memory
        /// </summary>
        private const int maxHistory = 100;

        /// <summary>
        /// Height of lines in pixels
        /// </summary>
        private const int lineHeight = 16;

        /// <summary>
        /// Input prompt, displayed in front of the user input
        /// </summary>
        private const string prompt = "> ";

        /// <summary>
        /// Cursor displayed at edit location
        /// </summary>
        private const string cursor = "_";


        /// <summary>
        /// Console text style (font mostly)
        /// </summary>
        private static GUIStyle style;

        /// <summary>
        /// Log of user input and command output
        /// </summary>
        private static DropOutStack<LogEntry> cmdLog;

        /// <summary>
        /// Command history for retreival with up and down arrows
        /// </summary>
        private static DropOutStack<String> history;

        /// <summary>
        /// Where the cursor is within the line
        /// </summary>
        private static int editLocation = 0;

        /// <summary>
        /// Where we are in the command history
        /// </summary>
        private static int historySelector = -1;

        /// <summary>
        /// What is currently in the input line
        /// </summary>
        private static string currentCmd = "";

        /// <summary>
        /// Command registry (name -> Command)
        /// </summary>
        private static Dictionary<string, Command> registry;

        /// <summary>
        /// Variable registry (name -> value)
        /// </summary>
        private static Dictionary<string, string> varRegistry;

        /// <summary>
        /// Is the console currently shown?
        /// </summary>
        public static bool show = false;

        /// <summary>
        /// Call this function before doing anything with the console
        /// </summary>
        public static void Init()
        {
            cmdLog = new DropOutStack<LogEntry>(maxHistory);
            history = new DropOutStack<string>(maxHistory);
            registry = new Dictionary<string, Command>();
            varRegistry = new Dictionary<string, string>();
            style = new GUIStyle
            {
                font = Font.CreateDynamicFontFromOSFont("Lucida Console", 16)
            };

            RegisterCommand(new Command_help());
            RegisterCommand(new Command_lsmod());
            RegisterCommand(new Command_set());
            Log("Console initialized");
            Log("Type \"help\" to get a list of commands");
        }

        /// <summary>
        /// Call this function on Update calls
        /// </summary>
        public static void Update()
        {
            // Toggle console with TAB
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                show = !show;
                if(SceneManager.GetActiveScene().name == "gameplay")
                {
                    if (show)
                        UIManager.UnlockMouseAndDisableFirstPersonLooking();
                    else if(!UIManager.SomeOtherMenuIsOpen)
                        UIManager.LockMouseAndEnableFirstPersonLooking();
                }
            }

            if (show)
            {
                // Handling history
                if (Input.GetKeyDown(KeyCode.UpArrow) && historySelector < history.Count - 1)
                {
                    historySelector += 1;
                    currentCmd = history.Get(historySelector);
                    editLocation = currentCmd.Length;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow) && historySelector > -1)
                {
                    historySelector -= 1;
                    if (historySelector == -1)
                        currentCmd = "";
                    else
                        currentCmd = history.Get(historySelector);
                    editLocation = currentCmd.Length;
                }
                // Handle editing
                if (Input.GetKeyDown(KeyCode.LeftArrow) && editLocation > 0)
                    editLocation--;
                if (Input.GetKeyDown(KeyCode.RightArrow) && editLocation < currentCmd.Length)
                    editLocation++;

                ReadInput(); // Read text input
            }
        }

        /// <summary>
        /// Call this function on OnGUI calls
        /// </summary>
        public static void Draw()
        {
            if (!show)
                return;

            Color background = Color.black;
            background.a = 0.5f;
            int height = Screen.height / 2;
            int width = Screen.width;
            int linecount = height / lineHeight;
            // Background rectangle
            ModUtilities.Graphics.DrawRect(new Rect(0, 0, width, linecount * lineHeight + 5), background);
            for(int line = 0; line < Math.Min(linecount - 1, cmdLog.Count); line++)
            {
                LogEntry entry = cmdLog.Get(line);
                int y = (linecount - 2 - line) * lineHeight;
                DrawText(entry.Message, new Vector2(5, y), entry.GetColor());
            }
            int consoleY = (linecount - 1) * lineHeight;
            try
            {
                DrawText(prompt + currentCmd, new Vector2(5, consoleY), Color.green);
                float x = Width(prompt) + Width(currentCmd.Substring(0, editLocation));
                DrawText(cursor, new Vector2(5 + x, consoleY), Color.green);
            } catch(Exception e)
            {
                Error($"currentCmd: \"{currentCmd}\"\neditLocation: {editLocation}");
                Error(e.ToString());
                currentCmd = "";
                editLocation = 0;
            }
        }

        /// <summary>
        /// Log a message to the console (can be multiline)
        /// </summary>
        /// <param name="type">Type of log <see cref="LogType"/></param>
        /// <param name="msg">Message to log</param>
        public static void Log(LogType type, string msg)
        {
            string[] lines = msg.Split('\n');
            foreach(string line in lines)
            {
                cmdLog.Push(new LogEntry(type, line));
            }
        }

        /// <summary>
        /// Logs a message as simple info
        /// </summary>
        /// <param name="msg">Message to log</param>
        public static void Log(string msg)
        {
            Log(LogType.INFO, msg);
        }

        /// <summary>
        /// Saves the value of a global variable
        /// </summary>
        /// <param name="variable">The variable to set</param>
        /// <param name="value">The value to give</param>
        public static void SetVariable(string variable, string value)
        {
            varRegistry.Add(variable, value);
        }

        /// <summary>
        /// Obtains the value of a global variable
        /// </summary>
        /// <param name="variable">The variable to get</param>
        /// <returns>The value, or null if variable is not set</returns>
        public static string GetVariable(string variable)
        {
            string value;
            if(varRegistry.TryGetValue(variable, out value))
                return value;
            return null;
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="msg">Message to log</param>
        public static void Error(string msg)
        {
            Log(LogType.ERROR, msg);
        }

        /// <summary>
        /// Register a command with a callback
        /// </summary>
        /// <param name="name">Name of the command. This is what is typed in the
        /// console to invoke the command</param>
        /// <param name="callback">The callback which is called when the command
        /// is invoked. Its arguments are the arguments of the command</param>
        /// <returns>True of succeeded, false otherwise</returns>
        public static bool RegisterCommand(Command command)
        {
            if (registry.ContainsKey(command.Name))
                return false;
            registry.Add(command.Name, command);
            return true;
        }

        /// <summary>
        /// Removes a command from the registry
        /// </summary>
        /// <param name="name">Name of the command to remove</param>
        /// <returns>True if a command was removed, false otherwise</returns>
        public static bool UnregisterCommand(string name)
        {
            if(registry.ContainsKey(name))
            {
                registry.Remove(name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called when the user presses enter
        /// </summary>
        /// <param name="cmd">The full command line</param>
        private static void ExecuteCommand(string cmd)
        {
            if (cmd.Length == 0)
                return;
            string[] words = cmd.Split(' ');
            string verb = words[0];
            Command command;
            if(registry.TryGetValue(verb, out command))
            {
                try
                {
                    command.Execute(words.Skip(1));
                }
                catch (Exception e)
                {
                    Log(LogType.ERROR, e.ToString());
                }
            }
            else
            {
                Log(LogType.ERROR, $"Unrecognized command: {verb}");
            }
        }

        private static void ReadInput()
        {
            foreach(char c in Input.inputString)
            {
                if (c == '\b') // has backspace/delete been pressed?
                {
                    if (currentCmd.Length != 0)
                    {
                        string firstHalf = currentCmd.Substring(0, editLocation - 1);
                        string secondHalf = currentCmd.Substring(editLocation, currentCmd.Length - editLocation);
                        currentCmd = firstHalf + secondHalf;
                        editLocation--;
                    }
                }
                else if (c == 0x7F) // Ctrl + Backspace (erase word)
                {
                    if (currentCmd.Length != 0)
                    {
                        int index = editLocation;
                        while(index > 0 && Char.IsLetterOrDigit(currentCmd.ElementAt(index - 1)))
                            index--;
                        if (index == editLocation && editLocation > 0) // Delete at least 1 character
                            index--;
                        int length = editLocation - index;
                        string firstHalf = currentCmd.Substring(0, index);
                        string secondHalf = currentCmd.Substring(editLocation, currentCmd.Length - editLocation);
                        currentCmd = firstHalf + secondHalf;
                        editLocation -= length;
                    }
                }
                else if ((c == '\n') || (c == '\r')) // enter/return
                {
                    Log(LogType.USERINPUT, "> " + currentCmd);
                    history.Push(currentCmd);
                    ExecuteCommand(currentCmd);
                    currentCmd = "";
                    editLocation = 0;
                }
                else
                {
                    currentCmd = currentCmd.Insert(editLocation, c.ToString());
                    editLocation++;
                }
            }
        }

        private static float Width(string text)
        {
            return style.CalcSize(new GUIContent(text)).x;
        }

        private static void DrawText(string text, Vector2 pos, Color color)
        {
            GUIStyle newStyle = new GUIStyle(style);
            newStyle.normal.textColor = color;
            Vector2 size = style.CalcSize(new GUIContent(text));
            Rect rect = new Rect(pos, size);

            GUI.Label(rect, text, newStyle);
        }

        private class Command_help : Command
        {
            public override string Name => "help";
            public override string Usage => $"{Name} [command]";
            public override string Description => "Lists command and shows their usage (help command)";

            public override void Execute(IEnumerable<string> arguments)
            {
                if(arguments.Count() == 0)
                {
                    foreach(Command command in registry.Values)
                    {
                        string log = command.Name;
                        if (command.Description != null)
                            log += ": " + command.Description;
                        Log(log);
                    }
                }
                else if(arguments.Count() == 1)
                {
                    string name = arguments.ElementAt(0);
                    Command command;
                    if(registry.TryGetValue(name, out command))
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
                    return;
                }
            }
        }

        private class Command_lsmod : Command
        {
            public override string Name => "lsmod";
            public override string Usage => $"{Name}";
            public override string Description => "Lists loaded mods (not implemented)";

            public override void Execute(IEnumerable<string> arguments)
            {
                throw new NotImplementedException();
            }
        }

        private class Command_set : Command
        {
            public override string Name => "set";
            public override string Usage => $"{Name} variable [value]";
            public override string Description => "Gets and sets global variables";

            public override void Execute(IEnumerable<string> arguments)
            {
                if(arguments.Count() == 1)
                {
                    string variable = arguments.ElementAt(0);
                    string value = Console.GetVariable(variable);
                    if (value != null)
                        Console.Log(value);
                    else
                        Console.Error($"Variable {variable} no set");
                }
                else if(arguments.Count() == 2)
                {
                    string variable = arguments.ElementAt(0);
                    string value = arguments.ElementAt(1);
                    Console.SetVariable(variable, value);
                }
                else
                {
                    Console.Error(Usage);
                }
            }
        }
    }
}
