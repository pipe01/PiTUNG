using System.Reflection;
using System;

namespace PiTung_Bootstrap
{
    /// <summary>
    /// Contains information about the current PiTUNG installation.
    /// </summary>
    public static class PiTUNG
    {
        /// <summary>
        /// The running PiTUNG version.
        /// </summary>
        public static Version FrameworkVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version;
    }
}
