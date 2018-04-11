using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung.Console
{
    static class Autocompletion
    {
        /// <summary>
        /// Returns the autocompletion candidates from the given command stub and available commands
        /// </summary>
        /// <param name="command">What needs to be autocompleted</param>
        /// <param name="commands">The possible values</param>
        /// <returns>The autocompletion candidates</returns>
        public static IEnumerable<String> Candidates(String command, IEnumerable<String> commands)
        {
            return commands.Where(candidate => candidate.StartsWith(command));
        }

        /// <summary>
        /// Computes the longest common prefix of a list of strings
        /// </summary>
        /// <param name="commands"></param>
        /// <returns>The longest common prefix of the collection of strings</returns>
        public static String CommonPrefix(IEnumerable<String> commands)
        {
            return commands.Aggregate((prefix, next) => LongestCommonPrefix(prefix, next));
        }

        /// <summary>
        /// Computes the longest common prefix of 2 strings
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The longest common prefix of a and b</returns>
        private static String LongestCommonPrefix(String a, String b)
        {
            String result = "";
            for (int index = 0; index < a.Length; index++)
            {
                if (a[index] != b[index])
                    return result;
                result += a[index];
            }
            return result;
        }
    }
}
