using System.Reflection;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PiTung.Console.CmdParser;

namespace PiTung.Console
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
        private static Dictionary<string, float> WordWidthCache = new Dictionary<string, float>();

        public LogType Type { get; }
        public string Message { get; }
        
        public LogEntry(LogType type, string message)
        {
            this.Type = type;
            this.Message = message;
        }

        public IEnumerable<float> GetWordWidths(GUIStyle style)
        {
            foreach (var item in Message.Split(' '))
            {
                if (!WordWidthCache.ContainsKey(item))
                    WordWidthCache[item] = style.CalcSize(new GUIContent(item)).x;

                yield return WordWidthCache[item];
            }
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
        /// If false, this command won't be shown when executing "help" on the console.
        /// </summary>
        internal virtual bool ShowOnHelp { get; } = true;

        /// <summary>
        /// The mod that registered this command.
        /// </summary>
        internal Mod Mod { get; set; }

        /// <summary>
        /// Called when the command is invoked
        /// </summary>
        /// <param name="arguments">The arguments given to the command</param>
        /// <returns>False if the command was malformed (i.e., the command requires 2 arguments but only 1 was supplied).</returns>
        public abstract bool Execute(IEnumerable<string> arguments);

        /// <summary>
        /// Called when the autocompletion is triggered
        /// </summary>
        /// <param name="arguments">The arguments to the command, the last one needing autocompletion</param>
        /// <returns>The autocompletion candidates for the last argument</returns>
        public virtual IEnumerable<String> AutocompletionCandidates(IEnumerable<String> arguments)
        {
            return new List<String>();
        }
    }

    //FIXME: Changing scene to gameplay locks mouse
    //TODO: Add verbosity levels

    /// <summary>
    /// In game console.
    /// <para>This static class allows for logging and registering commands
    /// which will be executed by callbacks. Contains a dictionary of
    /// global variables that can be read and written to from the console
    /// using the "set" command.</para>
    /// </summary>
    public static class IGConsole
    {
        /// <summary>
        /// Max number of log entries kept in memory
        /// </summary>
        private const int MaxHistory = 100;

        /// <summary>
        /// Height of lines in pixels
        /// </summary>
        private const int LineHeight = 16;

        /// <summary>
        /// Input prompt, displayed in front of the user input
        /// </summary>
        private const string Prompt = "> ";

        /// <summary>
        /// Cursor displayed at edit location
        /// </summary>
        private const string Cursor = "_";


        /// <summary>
        /// Console text style (font mostly)
        /// </summary>
        private static GUIStyle Style;

        /// <summary>
        /// Log of user input and command output
        /// </summary>
        private static DropOutStack<LogEntry> CmdLog;

        /// <summary>
        /// Command history for retrieval with up and down arrows
        /// </summary>
        private static DropOutStack<String> History;

        /// <summary>
        /// Where the cursor is within the line
        /// </summary>
        private static int EditLocation = 0;

        /// <summary>
        /// Where we are in the command history
        /// </summary>
        private static int HistorySelector = -1;

        /// <summary>
        /// What is currently in the input line
        /// </summary>
        private static string CurrentCmd = "";

        /// <summary>
        /// Command registry (name -> Command)
        /// </summary>
        internal static Dictionary<string, Command> Registry;

        /// <summary>
        /// Variable registry (name -> value)
        /// </summary>
        private static Dictionary<string, string> VarRegistry;

        /// <summary>
        /// Time at which the show-hide animation started.
        /// </summary>
        private static float ShownAtTime = 0;

        /// <summary>
        /// Is the console currently shown?
        /// </summary>
        internal static bool Shown = false;

        /// <summary>
        /// Show-hide animation duration.
        /// </summary>
        internal static float ShowAnimationTime = .3f;

        /// <summary>
        /// True if <see cref="Init"/> has been called.
        /// </summary>
        private static bool Initialized = false;

        private static UIState PreviousUIState = UIState.None;

        /// <summary>
        /// The queue of log entries that were added before initializing.
        /// </summary>
        private static Queue<KeyValuePair<LogType, object>> EntryQueue = new Queue<KeyValuePair<LogType, object>>();

        /// <summary>
        /// Call this function before doing anything with the console
        /// </summary>
        internal static void Init()
        {
            CmdLog = new DropOutStack<LogEntry>(MaxHistory);
            History = new DropOutStack<string>(MaxHistory);
            Registry = new Dictionary<string, Command>();
            VarRegistry = new Dictionary<string, string>();
            Style = new GUIStyle
            {
                font = Font.CreateDynamicFontFromOSFont("Consolas", 16),
                richText = true
            };

            LoadCommands();

            ModInput.RegisterBinding(null, "ToggleConsole", KeyCode.BackQuote);
            ModInput.RegisterBinding(null, "ConsoleAutocompletion", KeyCode.Tab);

            Initialized = true;

            Log("Console initialized");
            Log("Type \"help\" to get a list of commands");

            while (EntryQueue.Any())
            {
                KeyValuePair<LogType, object> entry = EntryQueue.Dequeue();
                Log(entry.Key, entry.Value);
            }
        }

        private static void LoadCommands()
        {
            foreach (var item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (item.Name.StartsWith("Command_", StringComparison.InvariantCulture) && item.BaseType == typeof(Command))
                {
                    RegisterCommand((Command)Activator.CreateInstance(item));
                }
            }
        }

        /// <summary>
        /// Call this function on Update calls
        /// </summary>
        internal static void Update()
        {
            // Toggle console with TAB
            if (ModInput.GetKeyDown("ToggleConsole"))
            {
                Shown = !Shown;

                float off = 0;

                //The user is toggling the console but the animation hasn't yet ended, resume it later
                if (Time.time - ShownAtTime < ShowAnimationTime)
                {
                    off -= ShowAnimationTime - (Time.time - ShownAtTime);
                }

                ShownAtTime = Time.time + off;

                if(SceneManager.GetActiveScene().name == "gameplay")
                {
                    if (Shown)
                    {
                        PreviousUIState = GameplayUIManager.UIState;
                        GameplayUIManager.UIState = UIState.PauseMenuOrSubMenu;
                    }
                    else
                    {
                        GameplayUIManager.UIState = PreviousUIState;
                        PreviousUIState = UIState.None;
                    }
                }
            }

            if (Shown)
            {
                if (ModInput.GetKeyDown("ConsoleAutocompletion"))
                    TriggerAutocompletion();

                // Handling history
                if (Input.GetKeyDown(KeyCode.UpArrow) && HistorySelector < History.Count - 1)
                {
                    HistorySelector += 1;
                    CurrentCmd = History.Get(HistorySelector);
                    EditLocation = CurrentCmd.Length;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow) && HistorySelector > -1)
                {
                    HistorySelector -= 1;
                    if (HistorySelector == -1)
                        CurrentCmd = "";
                    else
                        CurrentCmd = History.Get(HistorySelector);
                    EditLocation = CurrentCmd.Length;
                }
                // Handle editing
                if (Input.GetKeyDown(KeyCode.LeftArrow) && EditLocation > 0)
                    EditLocation--;
                if (Input.GetKeyDown(KeyCode.RightArrow) && EditLocation < CurrentCmd.Length)
                    EditLocation++;

                ReadInput(); // Read text input
            }
        }

        /// <summary>
        /// Call this function on OnGUI calls
        /// </summary>
        internal static void Draw()
        {
            if (!Shown && Time.time - ShownAtTime > ShowAnimationTime)
            {
                return;
            }
            
            Color background = Color.black;
            background.a = 0.75f;

            int height = Screen.height / 2;
            int width = Screen.width;
            int linecount = height / LineHeight;

            float yOffset = 0;

            if (Time.time - ShownAtTime < ShowAnimationTime)
            {
                int a = Shown ? height : 0;
                int b = Shown ? 0 : height;
                float val = (Time.time - ShownAtTime) / ShowAnimationTime;

                yOffset = -EaseOutQuad(a, b, val);

                if (!Shown)
                    val = 1 - val;

                background.a *= val;
            }

            // Background rectangle
            ModUtilities.Graphics.DrawRect(new Rect(0, yOffset, width, height + 5), background);

            for(int line = 0; line < Math.Min(linecount - 1, CmdLog.Count); line++)
            {
                LogEntry entry = CmdLog.Get(line);
                int y = (linecount - 2 - line) * LineHeight;
                DrawText(entry.Message, new Vector2(5, y + yOffset), entry.GetColor());
            }

            float consoleY = (linecount - 1) * LineHeight + yOffset;

            try
            {
                DrawText(Prompt + CurrentCmd, new Vector2(5, consoleY), Color.green);
                float x = Width(Prompt) + Width(CurrentCmd.Substring(0, EditLocation));
                DrawText(Cursor, new Vector2(5 + x, consoleY), Color.green);
            }
            catch (Exception e)
            {
                Error($"currentCmd: \"{CurrentCmd}\"\neditLocation: {EditLocation}");
                Error(e.ToString());
                CurrentCmd = "";
                EditLocation = 0;
            }

            float Width(string text) => Style.CalcSize(new GUIContent(text)).x;
        }

        /// <summary>
        /// Log a message to the console (can be multi-line)
        /// </summary>
        /// <param name="type">Type of log <see cref="LogType"/></param>
        /// <param name="msg">Message to log</param>
        public static void Log(LogType type, object msg)
        {
            if (!Initialized)
            {
                EntryQueue.Enqueue(new KeyValuePair<LogType, object>(type, msg));
                return;
            }

            string m = WordWrap(msg.ToString(), (int)(Screen.width / Style.CalcSize(new GUIContent("A")).x));
            
            foreach (string line in m.Split('\n'))
            {
                var logEntry = new LogEntry(type, line);

                CmdLog.Push(logEntry);
            }
        }

        /// <summary>
        /// Logs a message as simple info
        /// </summary>
        /// <param name="msg">Message to log</param>
        public static void Log(object msg)
        {
            Log(LogType.INFO, msg.ToString());
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
            if(VarRegistry.TryGetValue(variable, out value))
                return value;
            return null;
        }

        /// <summary>
        /// Returns the names of the variables in the registry
        /// </summary>
        /// <returns>An enumerable containing the names of the variables</returns>
        public static IEnumerable<String> GetVariables()
        {
            return VarRegistry.Keys;
        }

        /// <summary>
        /// Returns the names of the available commands
        /// </summary>
        /// <returns>An enumerable containing the names of the variables</returns>
        public static IEnumerable<String> GetCommandNames()
        {
            return Registry.Keys;
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="msg">Message to log</param>
        public static void Error(object msg)
        {
            Log(LogType.ERROR, msg);
        }

        /// <summary>
        /// Register a command.
        /// </summary>
        /// <param name="command">The command class.</param>
        internal static bool RegisterCommand(Command command)
        {
            if (Registry.ContainsKey(command.Name))
                return false;
            Registry.Add(command.Name, command);
            return true;
        }

        /// <summary>
        /// Register a command.
        /// </summary>
        /// <typeparam name="T">The type of the command.</typeparam>
        internal static bool RegisterCommand<T>() where T : Command
        {
            return RegisterCommand(Activator.CreateInstance<T>());
        }

        /// <summary>
        /// Register a command.
        /// </summary>
        /// <param name="command">The command class.</param>
        /// <param name="mod">The mod that is registering this command.</param>
        public static bool RegisterCommand(Command command, Mod mod)
        {
            command.Mod = mod;
            return RegisterCommand(command);
        }

        /// <summary>
        /// Register a command.
        /// </summary>
        /// <typeparam name="T">The type of the command.</typeparam>
        /// <param name="mod">The mod that is registering this command.</param>
        public static bool RegisterCommand<T>(Mod mod) where T : Command
        {
            return RegisterCommand(Activator.CreateInstance<T>(), mod);
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
        private static void ExecuteCommand(string cmd)
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

            if(Registry.TryGetValue(verb, out command))
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

        private static void ReadInput()
        {
            foreach(char c in Input.inputString)
            {
                if (c == '\b') // has backspace/delete been pressed?
                {
                    if (CurrentCmd.Length != 0)
                    {
                        string firstHalf = CurrentCmd.Substring(0, EditLocation - 1);
                        string secondHalf = CurrentCmd.Substring(EditLocation, CurrentCmd.Length - EditLocation);
                        CurrentCmd = firstHalf + secondHalf;
                        EditLocation--;
                    }
                }
                else if (c == 0x7F) // Ctrl + Backspace (erase word)
                {
                    if (CurrentCmd.Length != 0)
                    {
                        int index = EditLocation;
                        while (index > 0 && Char.IsLetterOrDigit(CurrentCmd.ElementAt(index - 1)))
                            index--;
                        if (index == EditLocation && EditLocation > 0) // Delete at least 1 character
                            index--;
                        int length = EditLocation - index;
                        string firstHalf = CurrentCmd.Substring(0, index);
                        string secondHalf = CurrentCmd.Substring(EditLocation, CurrentCmd.Length - EditLocation);
                        CurrentCmd = firstHalf + secondHalf;
                        EditLocation -= length;
                    }
                }
                else if ((c == '\n') || (c == '\r')) // enter/return
                {
                    if (!string.IsNullOrEmpty(CurrentCmd.Trim()))
                    {
                        Log(LogType.USERINPUT, "> " + CurrentCmd);
                        History.Push(CurrentCmd);
                        ExecuteCommand(CurrentCmd);
                    }

                    CurrentCmd = "";
                    EditLocation = 0;
                }
                else if ((KeyCode)c != ModInput.GetBindingKey("ToggleConsole"))
                {
                    CurrentCmd = CurrentCmd.Insert(EditLocation, c.ToString());
                    EditLocation++;
                }
            }
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
        /// Returns the autocompletion candidates for a given command.
        /// Depending on what is entered, it will either return command verbs
        /// or command-provided autocompletion candidates
        /// </summary>
        /// <param name="command">The current comand [verb, arg1, arg2,...]</param>
        /// <returns>The autocompletion candidates for the last argument</returns>
        private static IEnumerable<String> GetAutocompletionCandidates(IEnumerable<String> command)
        {
            if (command.Count() == 0)
                return new List<String>();

            String verb = command.ElementAt(0);
            if (command.Count() == 1)
                return Autocompletion.Candidates(verb, Registry.Keys);

            Command _command;
            if (Registry.TryGetValue(verb, out _command))
                return _command.AutocompletionCandidates(command.Skip(1));

            return new List<String>();
        }

        /// <summary>
        /// Saves the contents of the command when TriggetAutocompletion() was last called
        /// If it is the same, the next call to TriggerAutocompletion will log the 
        /// autocompletion candidates if there are multiple
        /// </summary>
        private static String PreviousCmd = "";

        /// <summary>
        /// Attempts to autocomplete the current word
        /// </summary>
        private static void TriggerAutocompletion()
        {
            List<Token> tokens = new List<Token>(LexString(CurrentCmd));
            if (tokens.Last().Type == TokenType.WHITESPACE) // If ends in whitespace, consider the next argument present but empty
                tokens.Add(new Token(TokenType.TEXT, ""));
            List<String> candidates = new List<String>(
                GetAutocompletionCandidates(
                    ConstructArguments(tokens)));

            bool finish_string = true;

            if (candidates.Count() == 0) // No candidate
                return;

            if (candidates.Count() == 1) // 1 candidate, autocomplete
            {
                if (tokens.Last().Type != TokenType.WHITESPACE)
                {
                    Token token = tokens.Last();
                    tokens[tokens.Count() - 1] = new Token(
                        ContainsSpaces(candidates[0]) ?  TokenType.QUOTE : token.Type,
                        candidates[0]);
                    tokens.Add(new Token(TokenType.WHITESPACE, " "));
                }
            }
            else
            {
                // More than 1 candidates, complete as much as possible and
                // display a list of candidates if necessary

                String common_prefix = Autocompletion.CommonPrefix(candidates);
                if (tokens.Last().Type != TokenType.WHITESPACE)
                {
                    Token token = tokens.Last();
                    tokens[tokens.Count() - 1] = new Token(
                        ContainsSpaces(common_prefix) ?  TokenType.QUOTE : token.Type,
                        common_prefix);
                    if (tokens.Last().Type == TokenType.QUOTE)
                        finish_string = CurrentCmd.Last() == '\"';
                }

                if (PreviousCmd == CurrentCmd)
                {
                    String list = "";
                    foreach (String candidate in candidates)
                        list += candidate + "    ";
                    Log(list);
                }
                PreviousCmd = CurrentCmd;
            }

            String reconstruction = Reconstruct(tokens);
            if (!finish_string)
                CurrentCmd = reconstruction.Remove(reconstruction.Length - 1);
            else
                CurrentCmd = reconstruction;
            EditLocation = CurrentCmd.Length;
        }

        private static float EaseOutQuad(float start, float end, float value)
        {
            end -= start;
            return -end * value * (value - 2) + start;
        }
        
        private static void DrawText(string text, Vector2 pos, Color color)
        {
            GUIStyle newStyle = new GUIStyle(Style);
            newStyle.normal.textColor = color;
            Vector2 size = Style.CalcSize(new GUIContent(text));
            Rect rect = new Rect(pos, size);

            GUI.Label(rect, text, newStyle);
        }

        static char[] splitChars = new char[] { ' ', '-', '\t' };
        private static string WordWrap(string str, int width)
        {
            string[] words = Explode(str, splitChars);

            int curLineLength = 0;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                // If adding the new word to the current line would be too long,
                // then put it on a new line (and split it up if it's too long).
                if (curLineLength + word.Length > width)
                {
                    // Only move down to a new line if we have text on the current line.
                    // Avoids situation where wrapped whitespace causes empty lines in text.
                    if (curLineLength > 0)
                    {
                        strBuilder.Append('\n');
                        curLineLength = 0;
                    }

                    // If the current word is too long to fit on a line even on it's own then
                    // split the word up.
                    while (word.Length > width)
                    {
                        strBuilder.Append(word.Substring(0, width - 1) + "-");
                        word = word.Substring(width - 1);

                        strBuilder.Append('\n');
                    }

                    // Remove leading whitespace from the word so the new line starts flush to the left.
                    word = word.TrimStart();
                }
                strBuilder.Append(word);
                curLineLength += word.Length;
            }

            return strBuilder.ToString();
        }

        private static string[] Explode(string str, char[] splitChars)
        {
            List<string> parts = new List<string>();
            int startIndex = 0;
            while (true)
            {
                int index = str.IndexOfAny(splitChars, startIndex);

                if (index == -1)
                {
                    parts.Add(str.Substring(startIndex));
                    return parts.ToArray();
                }

                string word = str.Substring(startIndex, index - startIndex);
                char nextChar = str.Substring(index, 1)[0];
                // Dashes and the likes should stick to the word occurring before it. Whitespace doesn't have to.
                if (char.IsWhiteSpace(nextChar))
                {
                    parts.Add(word);
                    parts.Add(nextChar.ToString());
                }
                else
                {
                    parts.Add(word + nextChar);
                }

                startIndex = index + 1;
            }
        }
    }
}
