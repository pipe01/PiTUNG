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
                yield return GetMod(item);
            }
        }
        
        /// <summary>
        /// Loads a mod contained in the DLL pointed by <paramref name="modPath"/>.
        /// </summary>
        /// <param name="modPath">The mod's DLL file path.</param>
        /// <returns>A mod.</returns>
        private static Mod GetMod(string modPath)
        {
            var ass = Assembly.LoadFrom(modPath);

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
