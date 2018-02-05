namespace PiTung_Bootstrap
{
    internal static class MDebug
    {
        public static int DebugLevel { get; set; } = 10;

        public static void WriteLine(string line)
        {
            System.Console.WriteLine("[PiTUNG] " + line);
        }

        public static void WriteLine(object line, int level = 0, params object[] args)
        {
            if (level <= DebugLevel)
                WriteLine(string.Format(line.ToString(), args: args));
        }
    }
}
