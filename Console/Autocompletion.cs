using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung.Console
{
    static class Autocompletion
    {
        public static IEnumerable<String> Candidates(String command, IEnumerable<String> commands)
        {
            return commands.Where(candidate => candidate.StartsWith(command));
        }
    }
}
