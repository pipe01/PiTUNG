using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PiTung_Bootstrap
{
    internal static class ModLoader
    {
        public const string ModsDirectory = "./mods";

        public static IEnumerable<Mod> GetMods()
        {
            if (!Directory.Exists(ModsDirectory))
                Directory.CreateDirectory(ModsDirectory);

            foreach (var item in Directory.GetFiles(ModsDirectory, "*.dll"))
            {
                yield return GetMod(item);
            }
        }
        
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
