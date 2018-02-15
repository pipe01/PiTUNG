using UnityEngine;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
                { "0Harmony", Properties.Resources._0Harmony }
            };

            if (asses.TryGetValue(new AssemblyName(args.Name).Name, out var b))
            {
                return Assembly.Load(b);
            }
            
            return null;
        }
    }
}
