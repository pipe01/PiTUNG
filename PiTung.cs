using System.Reflection;
using System;

namespace PiTung_Bootstrap
{
    public static class PiTung
    {
        public static Version FrameworkVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;
    }
}
