namespace PiTung
{
    internal static class MDebug
    {
        public static int DebugLevel { get; set; }
#if DEBUG
            = 10;
#endif

        public static void WriteLine(object line)
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
