using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap
{
    public static class PiTung
    {
        public static Version FrameworkVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;
    }
}
