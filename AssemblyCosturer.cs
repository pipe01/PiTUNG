using UnityEngine;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PiTung
{
    public static class AssemblyCosturer
    {
        public static void Init()
        {
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
