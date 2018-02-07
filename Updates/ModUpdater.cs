using System.Net;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.IO;
using PiTung_Bootstrap.Console;
using System.Collections;

namespace PiTung_Bootstrap.Updates
{
    internal static class ModUpdater
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
        
        private static Dictionary<Mod, Manifest.ModInfo> ModInfos = new Dictionary<Mod, Manifest.ModInfo>();
        private static WebClient Client = new WebClient();

        public static void CheckUpdatesForMod(Mod mod, bool update)
        {
            if (string.IsNullOrEmpty(mod.UpdateUrl?.Trim()))
                return;

            var modInfo = GetModInfo(mod);

            bool updateAvail = modInfo.Version > mod.ModVersion;
            mod.HasAvailableUpdate = updateAvail;
            
            if (updateAvail)
            {
                Bootstrapper.Instance.ModUpdatesAvailable = true;

                if (update)
                    UpdateMod(mod);

                IGConsole.Log($"<color=lime>Downloaded update for {mod.FullName}!</color> Restart TUNG to install.");
            }
        }

        public static void UpdateMod(Mod mod)
        {
            GetModInfo(mod);
            
            if (ModInfos.TryGetValue(mod, out var val))
            {
                string rootUrl = (mod.UpdateUrl + "|").Replace($"/{Path.GetFileName(mod.UpdateUrl)}|", "/");
                string url = rootUrl + (val.FileName ?? Path.GetFileName(mod.FullPath));

                Client.DownloadFile(url, Path.Combine(Path.GetDirectoryName(mod.FullPath), Path.GetFileName(mod.FullPath)) + ".update");
                
                mod.HasAvailableUpdate = false;
            }
        }

        private static Manifest.ModInfo GetModInfo(Mod mod)
        {
            if (!ModInfos.ContainsKey(mod))
            {
                Manifest man = null;

                try
                {
                    string manText = Client.DownloadString(mod.UpdateUrl);
                    man = ManifestParser.ParseManifest(manText.Split('\n'));
                }
                catch (Exception ex)
                {
                    MDebug.WriteLine("Exception occurred while getting update manifest for " + mod.FullName);
                    MDebug.WriteLine("Details: " + ex);

                    IGConsole.Log("Error occurred while updating " + mod.FullName);
                }

                if (man == null || man.Mods == null || man.Mods.Length == 0)
                    return null;

                var modInfo = man.Mods.SingleOrDefault(o => o.Name.Equals(mod.Name));

                if (modInfo != null)
                    ModInfos[mod] = modInfo;
            }

            return ModInfos[mod];
        }
    }
}
