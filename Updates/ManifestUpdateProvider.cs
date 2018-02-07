using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiTung_Bootstrap.Console;
using System.IO;

namespace PiTung_Bootstrap.Updates
{
    public class ManifestUpdateProvider : UpdateProvider
    {
        public class Manifest
        {
            public class ModInfo
            {
                public string Package;
                public string FileName;
                public Version Version;
            }

            public ModInfo[] Mods { get; set; }
        }
        
        private static WebClient Client = new WebClient();
        
        public string ManifestUrl { get; }

        public ManifestUpdateProvider(string manifestUrl, string modPackage)
            : base(modPackage)
        {
            this.ManifestUrl = manifestUrl;
        }
        
        internal override Update GetUpdate()
        {
            var modinfo = GetModInfo();

            if (modinfo != null)
            {
                string rootUrl = (ManifestUrl + "|").Replace($"/{Path.GetFileName(ManifestUrl)}|", "/");
                string url = rootUrl + (modinfo.FileName ?? Path.GetFileName(LocalMod.FullPath));

                return new Update(ModPackage, modinfo.Version, url);
            }

            return default(Update);
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
                MDebug.WriteLine($"Exception occurred while getting update manifest for {ModPackage}.");
                MDebug.WriteLine("Details: " + ex);

                IGConsole.Log($"Error occurred while updating {ModPackage}.");
            }

            if (man == null || man.Mods == null || man.Mods.Length == 0)
                return null;

            return man.Mods.SingleOrDefault(o => o.Package.Equals(ModPackage));
        }
    }
}
