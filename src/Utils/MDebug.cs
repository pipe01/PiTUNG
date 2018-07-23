using System;
using System.IO;
using System.Text;
using UnityEngine;
using SConsole = System.Console;

namespace PiTung
{
    internal static class MDebug
    {
        public static int DebugLevel { get; set; }
#if DEBUG
            = 10;
#endif

        private const string LogPath = "logs/PiTUNG.txt";
        private const string OldLogPath = "logs/PiTUNG.old.txt";
        
        private static StreamWriter LogWriter;

        static MDebug()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath));

            if (File.Exists(LogPath))
            {
                if (File.Exists(OldLogPath))
                    File.Delete(OldLogPath); //TODO Keep it?

                File.Move(LogPath, OldLogPath);
            }

            LogWriter = new StreamWriter(LogPath);
            LogWriter.AutoFlush = true;
            
            SConsole.SetOut(new CustomTextWriter(SConsole.Out, LogWriter));
            Application.logMessageReceived += LogCallback;
        }

        private static void LogCallback(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
                LogWriter.WriteLine("[EXCEPTION] {1}", stackTrace);
        }

        public static void WriteLine(object line)
        {
            SConsole.WriteLine("[PiTUNG] " + line);
        }

        public static void WriteLine(object line, int level = 0, params object[] args)
        {
            if (level <= DebugLevel)
                WriteLine(string.Format(line.ToString(), args: args));
        }

        private class CustomTextWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;

            private readonly TextWriter ConsoleOut;
            private readonly TextWriter LogOut;

            public CustomTextWriter(TextWriter consoleOut, TextWriter logOut)
            {
                this.ConsoleOut = consoleOut;
                this.LogOut = logOut;
            }

            public override void Write(char value)
            {
                ConsoleOut.Write(value);
                LogOut.Write(value);
            }

            public override void WriteLine(string value)
            {
                ConsoleOut.WriteLine(value);
                LogOut.WriteLine(value);
            }
        }
    }
}
