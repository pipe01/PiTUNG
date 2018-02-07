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
        private static WebClient Client = new WebClient();

        public static void CheckUpdatesForMod(Mod mod, bool update)
        {
            if (mod.UpdateProvider == null)
                return;

            IGConsole.Log(mod.Name);

            if (mod.HasAvailableUpdate = mod.UpdateProvider.IsUpdateAvailable())
            {
                Bootstrapper.Instance.ModUpdatesAvailable = true;

                if (update)
                    UpdateMod(mod);

                IGConsole.Log($"<color=lime>Downloaded update for {mod.FullName}!</color> Restart TUNG to install.");
            }
        }

        public static void UpdateMod(Mod mod)
        {
            if (!mod.HasAvailableUpdate)
                return;

            var provider = mod.UpdateProvider;

            if (provider == null)
                return;

            string updatePath = "mods/" + Path.GetFileName(mod.FullPath) + ".update";

            Client.DownloadFile(provider.GetUpdate().DownloadUrl, updatePath);

            mod.HasAvailableUpdate = false;
        }
    }
}
