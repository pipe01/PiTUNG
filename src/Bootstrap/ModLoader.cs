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

                mod.RequiredModPackages = mod.GetType().GetCustomAttributes(false)
                    .OfType<RequireModAttribute>()
                    .Select(o => o.ModPackage)
                    .ToArray();
                
                if (mod != null)
                    yield return mod;
            }
        }

        public static IEnumerable<Mod> SearchForCircularReferences(IEnumerable<Mod> mods)
        {
            Dictionary<string, Mod> modsByPackage = mods.ToDictionary(o => o.PackageName);
            List<KeyValuePair<Mod, Mod>> warned = new List<KeyValuePair<Mod, Mod>>();
            
            foreach (var item in mods)
            {
                var match = item.RequiredModPackages.SingleOrDefault(o => modsByPackage.TryGetValue(o, out var m) && m.RequiredModPackages.Contains(item.PackageName));

                if (match != null)
                {
                    var second = modsByPackage[match];

                    if (!warned.Any(o =>
                        (o.Key.PackageName == item.PackageName && o.Value.PackageName == second.PackageName) || 
                        (o.Key.PackageName == second.PackageName && o.Value.PackageName == item.PackageName)))
                    {
                        warned.Add(new KeyValuePair<Mod, Mod>(item, second));

                        IGConsole.Error($"Circular reference found between <b>{item.Name}</b> and <b>{second.Name}</b>, neither were loaded.");
                    }
                }
                else
                {
                    yield return item;
                }
            }
        }
        
        public static IEnumerable<Mod> Order(IEnumerable<Mod> mods)
        {
            mods = SearchForCircularReferences(mods).ToList();

            Dictionary<string, Mod> modsByPackage = mods.ToDictionary(o => o.PackageName);
            List<Mod> ret = new List<Mod>();

            foreach (var item in mods)
            {
                Push(item);
            }

            ret.AddRange(mods.Where(o => !ret.Contains(o)));

            return ret;

            void Push(Mod mod)
            {
                bool exit = true;

                foreach (var item in mod.RequiredModPackages)
                {
                    if (!modsByPackage.TryGetValue(item, out var req))
                    {
                        IGConsole.Error($"Mod {mod.Name} requires mod {item}");
                        exit = true;
                    }

                    if (req != null && !exit && !ret.Contains(req))
                    {
                        ret.Add(req);
                    }
                }
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

            var preloader = ass.GetType("Preloader");
            var preload = preloader?.GetMethod("Preload", BindingFlags.Public | BindingFlags.Static);

            if (preload != null)
                preload.Invoke(null, null);

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
