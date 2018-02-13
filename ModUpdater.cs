using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PiTung.Console;
using System.Collections;

namespace PiTung
{
    /// <summary>
    /// Handles the updating of mods.
    /// </summary>
    public static class ModUpdater
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

        internal static IEnumerator CheckUpdatesForMod(Mod mod, bool update)
        {
            if (mod.UpdateUrl == null)
                yield break;

            yield return GetModInfo(mod);

            if (ModInfos.TryGetValue(mod, out var modInfo))
                mod.HasAvailableUpdate = modInfo.Version > mod.ModVersion;

            if (mod.HasAvailableUpdate)
            {
                Bootstrapper.Instance.ModUpdatesAvailable = true;

                if (update)
                    yield return UpdateMod(mod);

                IGConsole.Log($"<color=lime>Downloaded update for {mod.FullName}!</color> Restart TUNG to install.");
            }
        }

        internal static IEnumerator UpdateMod(Mod mod)
        {
            yield return GetModInfo(mod);
            
            if (ModInfos.TryGetValue(mod, out var val))
            {
                string url = Path.Combine(mod.UpdateUrl, val.FileName ?? Path.GetFileName(mod.FullPath));

                var down = new WWW(url);
                yield return down;

                File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(mod.FullPath), Path.GetFileName(mod.FullPath)) + ".update", down.bytes);

                mod.HasAvailableUpdate = false;
            }
        }

        private static IEnumerator GetModInfo(Mod mod)
        {
            Manifest man = null;

            var down = new WWW(mod.UpdateUrl);
            yield return down;
            
            try
            {
                man = ManifestParser.ParseManifest(down.text.Split('\n'));
            }
            catch (Exception ex)
            {
                MDebug.WriteLine("Exception occurred while parsing update manifest for " + mod.FullName);
                MDebug.WriteLine("Details: " + ex);

                IGConsole.Log("Error occurred while updating " + mod.FullName);
            }
            
            if (man == null || man.Mods == null || man.Mods.Length == 0)
                yield break;

            var modInfo = man.Mods.SingleOrDefault(o => o.Name.Equals(mod.PackageName));

            if (modInfo != null)
                ModInfos[mod] = modInfo;
        }
    }
}
