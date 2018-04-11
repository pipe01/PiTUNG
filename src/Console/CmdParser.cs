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
            QUOTE, // For string literals
            TEXT, // For any strings not containing whitespaces
            ERROR // Error token, when input could not be lexed
        }

        internal struct Token
        {
            public TokenType Type;
            public String Value;

            public Token(TokenType Type, String Value) {
                this.Type = Type;
                this.Value = Value;
            }
        }

        internal static Regex WhiteSpace = new Regex("^\\s+");
        internal static Regex TextMatcher = new Regex("^(?:[^\\\\\" ]|\\\\\")+"); //\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\ all this to match \"
        internal static Regex StringMatcher = new Regex("^\"((?:[^\\\\\"]|\\\\\")+)\"?");

        /// <summary>
        /// Reads the next token in input
        /// </summary>
        /// <param name="input">The string to lex, will be eaten away</param>
        /// <returns>The next token in input</returns>
        internal static Token LexToken(ref String input)
        {
            Match str_match = StringMatcher.Match(input);
            if(str_match.Success)
            {
                String matched = str_match.Groups[1].Captures[0].Value;
                input = input.Remove(0, str_match.Value.Length);
                return new Token(TokenType.QUOTE, matched);
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

        /// <summary>
        /// Turns a string into a collection of tokens representing it
        /// </summary>
        /// <param name="command">The string to lex</param>
        /// <returns>A collection of tokens representing the string</returns>
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

        /// <summary>
        /// Turns a collection of tokens back into the string it represented
        /// </summary>
        /// <param name="tokens">The tokens to re-assemble</param>
        /// <returns>The assembled string</returns>
        public static String Reconstruct(IEnumerable<Token> tokens)
        {
            String result = "";
            foreach (Token token in tokens)
            {
                if (token.Type == TokenType.QUOTE)
                    result += "\"" + token.Value + "\"";
                else
                    result += token.Value;
            }
            return result;
        }

        /// <summary>
        /// Turns a collection of tokens into a collection of command arguments in the form [verb, arg1, arg2,...]
        /// </summary>
        /// <param name="tokens">The token to parse</param>
        /// <returns>The collection of arguments</returns>
        public static IEnumerable<String> ConstructArguments(IEnumerable<Token> tokens)
        {
            return tokens
                .Where(token => token.Type != TokenType.WHITESPACE)
                .Select(token => token.Value);
        }

        public static bool ContainsSpaces(String str)
        {
            return str.IndexOf(' ') >= 0;
        }
    }
}
