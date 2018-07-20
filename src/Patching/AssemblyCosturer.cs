using Harmony;
using PiTung.Properties;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PiTung
{
    /// <summary>
    /// This class allows PiTUNG to not require any external DLLs apart from those that belong to the game.
    /// This shouldn't be used in your code.
    /// </summary>
    public static class AssemblyCosturer
    {
        private static bool Initialized;

        public static void Init()
        {
            if (Initialized)
                return;

            Initialized = true;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Test();
        }

        private static void Test()
        {
            HarmonyInstance inst = null;
            try
            {
                inst = HarmonyInstance.Create("test");
            }
            catch (TypeLoadException)
            {
            }
            finally
            {
                if (inst == null)
                {
                    MDebug.WriteLine("!!!Failed to load assemblies!!!");
                }
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var asses = new Dictionary<string, byte[]>
            {
                ["0Harmony"] = Resources._0Harmony,
                ["fastJSON"] = Resources.fastJSON,
                ["System.Data"] = Resources.System_Data,
                ["System.Xml"] = Resources.System_XML,
            };

            string name = new AssemblyName(args.Name).Name;

            if (asses.TryGetValue(name, out var b))
            {
                Assembly ass = null;

                try
                {
                    ass = Assembly.Load(b);
                    MDebug.WriteLine($"Successfully loaded {name} from resources");
                }
                catch (Exception ex)
                {
                    MDebug.WriteLine($"Failed to load assembly {name} from resources: {ex}");
                }

                return ass;
            }

            return null;
        }
    }
}
