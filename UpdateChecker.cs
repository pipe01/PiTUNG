using System.IO;
using System;
using UnityEngine;
using System.Collections;

namespace PiTung_Bootstrap
{
    internal static class UpdateChecker
    {
        private const string VersionFileUrl = "http://pipe0481.heliohost.org/pitung/version.txt";
        private const string InstallerUrl = "http://pipe0481.heliohost.org/pitung/Installer.exe";

        public delegate void UpdateStatusDelegate(bool updateAvailable, Version newVersion);
        public static event UpdateStatusDelegate UpdateStatus;
        
        public static bool IsUpdateAvailable { get; private set; }

        public static IEnumerator CheckUpdates()
        {
            var www = new WWW(VersionFileUrl);
            yield return www;

            var ver = new Version(www.text);
            bool avail = ver > PiTung.FrameworkVersion;

            IsUpdateAvailable = avail;
            UpdateStatus?.Invoke(avail, ver);

            if (avail)
                yield return DownloadInstaller();
        }

        private static IEnumerator DownloadInstaller()
        {
            var www = new WWW(InstallerUrl);
            yield return www;

            File.WriteAllBytes("Installer.exe", www.bytes);
        }
    }
}
