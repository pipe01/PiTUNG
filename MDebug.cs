using System;

namespace PiTung_Bootstrap
{
    internal static class MDebug
    {
        public static int DebugLevel { get; set; } = 10;

        public static void WriteLine(string line, int level = 0, params object[] args)
        {
            if (level <= DebugLevel)
                Console.WriteLine("[PiTung] " + string.Format(line, args: args));
        }
    }
}
