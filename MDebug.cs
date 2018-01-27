namespace PiTung_Bootstrap
{
    internal static class MDebug
    {
        public static int DebugLevel { get; set; } = 10;

        public static void WriteLine(object line, int level = 0, params object[] args)
        {
            if (level <= DebugLevel)
                System.Console.WriteLine("[PiTung] " + string.Format(line.ToString(), args: args));
        }
    }
}
