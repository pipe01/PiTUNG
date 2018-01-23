using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap.Console
{
    internal static class CmdParser
    {
        public static bool TryParseCmdLine(string line, out string verb, out string[] arguments, out string error)
        {
            List<string> args = new List<string>();

            string currentString = "";
            bool inQuotes = false;
            
            verb = line.Split(' ')[0];
            arguments = new string[0];

            if (string.IsNullOrEmpty(verb))
            {
                error = "empty command";
                return false;
            }

            line = line.Substring(verb.Length).Trim();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == ' ' && !inQuotes)
                {
                    args.Add(currentString.Trim());
                    currentString = "";
                }
                else if (c == '"')
                {
                    if (inQuotes)
                    {
                        args.Add(currentString.Trim());
                        currentString = "";
                    }

                    inQuotes = !inQuotes;
                }
                else
                {
                    currentString += c;
                }
            }

            if (currentString != "")
            {
                args.Add(currentString.Trim());
            }

            if (inQuotes)
            {
                error = "missing closing quote";
                return false;
            }

            args.RemoveAll(String.IsNullOrEmpty);

            arguments = args.ToArray();

            error = null;
            return true;
        }
    }
}
