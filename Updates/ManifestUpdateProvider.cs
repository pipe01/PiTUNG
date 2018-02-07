using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiTung_Bootstrap.Console;
using System.IO;

namespace PiTung_Bootstrap.Updates
{
    public class ManifestUpdateProvider : IUpdateProvider
    {
        public class Manifest
        {
            public class ModInfo
            {
                public string Name;
                public string FileName;
                public Version Version;
            }

            public ModInfo[] Mods { get; set; }
        }
        
        private static WebClient Client = new WebClient();

        private Update? LastUpdate;

        public string ManifestUrl { get; set; }
        public string ModName { get; set; }

        public ManifestUpdateProvider(string manifestUrl, string modName)
        {
            this.ManifestUrl = manifestUrl;
            this.ModName = modName;
        }

        private Mod GetMod() => Bootstrapper.Mods.SingleOrDefault(o => o.Name.Equals(ModName, StringComparison.InvariantCulture));

        public Update GetUpdate()
        {
            var modinfo = GetModInfo();

            if (modinfo != null)
            {
                string rootUrl = (ManifestUrl + "|").Replace($"/{Path.GetFileName(ManifestUrl)}|", "/");
                string url = rootUrl + (modinfo.FileName ?? Path.GetFileName(GetMod().FullPath));

                return new Update(ModName, modinfo.Version, url);
            }

            return default(Update);
        }

        public bool IsUpdateAvailable()
        {
            if (LastUpdate == null)
                LastUpdate = GetUpdate();

            var mod = GetMod();

            return LastUpdate.Value.NewVersion > mod.ModVersion;
        }

        private Manifest.ModInfo GetModInfo()
        {
            Manifest man = null;

            try
            {
                string manText = Client.DownloadString(ManifestUrl);

                man = ManifestParser.ParseManifest(manText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
            }
            catch (Exception ex)
            {
                MDebug.WriteLine($"Exception occurred while getting update manifest for {ModName}.");
                MDebug.WriteLine("Details: " + ex);

                IGConsole.Log($"Error occurred while updating {ModName}.");
            }

            if (man == null || man.Mods == null || man.Mods.Length == 0)
                return null;

            return man.Mods.SingleOrDefault(o => o.Name.Equals(ModName));
        }
    }
}
