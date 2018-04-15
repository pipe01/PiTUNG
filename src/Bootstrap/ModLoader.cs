using Harmony;
using PiTung.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PiTung
{
    internal static class ModLoader
    {
        public const string ModsDirectory = "./mods";

        /// <summary>
        /// Search for mods on <see cref="ModsDirectory"/>.
        /// </summary>
        /// <returns>Mods.</returns>
        public static IEnumerable<Mod> GetMods()
        {
            if (!Directory.Exists(ModsDirectory))
                Directory.CreateDirectory(ModsDirectory);
            
            foreach (var item in Directory.GetFiles(ModsDirectory, "*.dll"))
            {
                if (Path.GetFileNameWithoutExtension(item).EndsWith("-disabled"))
                    continue;

                Mod mod = null;

                try
                {
                    mod = GetMod(item);
                }
                catch (Exception ex)
                {
                    string name = Path.GetFileNameWithoutExtension(item);
                    MDebug.WriteLine($"[ERROR] Mod {name} failed to load.");
                    MDebug.WriteLine("More details:\n" + ex, 2);
                    IGConsole.Error($"Failed to load mod {name}.");
                }

                if (mod != null)
                    yield return mod;
            }
        }

        /// <summary>
        /// Loads a mod contained in the DLL pointed by <paramref name="modPath"/>.
        /// </summary>
        /// <param name="modPath">The mod's DLL file path.</param>
        /// <returns>A mod.</returns>
        public static Mod GetMod(string modPath)
        {
            if (File.Exists(modPath + ".update"))
            {
                File.Delete(modPath);
                File.Move(modPath + ".update", modPath);
            }

            Assembly ass = Assembly.Load(File.ReadAllBytes(modPath));

            Mod mod = null;

            foreach (var item in ass.GetExportedTypes())
            {
                if (item.BaseType == typeof(Mod))
                {
                    mod = Activator.CreateInstance(item) as Mod;

                    mod.ModAssembly = ass;
                    mod.FullPath = Path.GetFullPath(modPath);
                }
            }

            foreach (var item in ass.GetTypes())
            {
                //Reloadable if there are no raw Harmony patches
                mod.Reloadable = item.GetCustomAttributes(typeof(HarmonyPatch), false).Length == 0;
            }

            return mod;
        }
    }
}
