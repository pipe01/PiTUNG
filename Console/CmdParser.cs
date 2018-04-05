using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PiTung.Console
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
                else if (c == '"' && (i == 0 || line[i - 1] != '\\'))
                {
                    if (inQuotes)
                    {
                        args.Add(currentString.Trim());
                        currentString = "";
                    }

                    inQuotes = !inQuotes;
                }
                else if (c != '\\')
                {
                    currentString += c;
                }
            }

            if (currentString != "")
            {
                args.Add(currentString.Trim());
            }
            
            args.RemoveAll(String.IsNullOrEmpty);

            arguments = args.ToArray();

            error = null;
            return true;
        }

        internal enum TokenType
        {
            WHITESPACE, // For any length of whitespace
            QUOTE, // For the " char
            TEXT, // For any strings not containing whitespaces
            ERROR // Error token, when input could not be lexed
        }

        internal struct Token
        {
            public readonly TokenType Type;
            public readonly String Value;

            public Token(TokenType Type, String Value) {
                this.Type = Type;
                this.Value = Value;
            }
        }

        internal static Regex WhiteSpace = new Regex("^\\s+");
        internal static Regex TextMatcher = new Regex("^(?:[^\" ]|\\\\\")+"); //\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\ all this to match \"
        internal static Regex StringMatcher = new Regex("^\"(?:[^\"]|\\\\\")+\"?");

        internal static Token LexToken(ref String input)
        {
            if(input[0] == '"')
            {
                input = input.Remove(0, 1);
                return new Token(TokenType.QUOTE, "\"");
            }
            Match ws_match = WhiteSpace.Match(input);
            if(ws_match.Success)
            {
                String matched = ws_match.Value;
                input = input.Remove(0, matched.Length);
                return new Token(TokenType.WHITESPACE, matched);
            }
            Match text_match = TextMatcher.Match(input);
            if(text_match.Success)
            {
                String matched = text_match.Value;
                input = input.Remove(0, matched.Length);
                return new Token(TokenType.TEXT, matched);
            }
            return new Token(TokenType.ERROR, "");
        }

        public static IEnumerable<Token> LexString(String command)
        {
            String input = command;
            List<Token> tokens = new List<Token>();
            while(input.Length > 0)
            {
                Token token = LexToken(ref input);
                if (token.Type == TokenType.ERROR)
                    break;
                tokens.Add(token);
            }
            return tokens;
        }

        public static bool EndsInsideQuotes(IEnumerable<Token> tokens)
        {
            return tokens
                .Where(token => token.Type == TokenType.QUOTE)
                .Count() % 2 == 1;
        }

        public static String Reconstruct(IEnumerable<Token> tokens)
        {
            String result = "";
            foreach (Token token in tokens)
                result += token.Value;
            return result;
        }

        public static IEnumerable<String> ConstructArguments(IEnumerable<Token> tokens)
        {
            bool in_quotes = false;
            List<String> arguments = new List<String>();
            String current = "";
            foreach (Token token in tokens)
            {
                if(token.Type == TokenType.QUOTE)
                {
                    if(in_quotes)
                    {
                        arguments.Add(current);
                        current = "";
                    }

                    in_quotes = !in_quotes;
                }
                else if(in_quotes)
                    current += token.Value;
                else if(token.Type == TokenType.TEXT)
                    arguments.Add(token.Value);
            }
            return arguments;
        }

        public static IEnumerable<String> PartialParse(String command)
        {
            return ConstructArguments(LexString(command));
        }
    }
}
