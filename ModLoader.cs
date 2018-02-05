using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PiTung_Bootstrap
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
                    MDebug.WriteLine($"[ERROR] Mod {Path.GetFileName(item)} failed to load.");
                    MDebug.WriteLine("More details: " + ex.Message, 2);
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
        private static Mod GetMod(string modPath)
        {
            if (File.Exists(modPath + ".update"))
            {
                File.Delete(modPath);
                File.Move(modPath + ".update", modPath);
            }

            Assembly ass = Assembly.LoadFrom(modPath);

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

            return mod;
        }
    }
}
