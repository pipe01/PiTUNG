using System.IO;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using PiTung_Bootstrap.Console;

namespace PiTung_Bootstrap
{
    public class Bootstrapper
    {
        private static bool Patched = false;

        public static int ModCount = 0;

        /// <summary>
        /// Main bootstrap method. Loads and patches all mods.
        /// </summary>
        public void Patch()
        {
            if (Patched)
                return;
            Patched = true;
            ModCount = 0;
            
            MDebug.WriteLine("PiTUNG Framework version {0}", 0, PiTung.FrameworkVersion);
            MDebug.WriteLine("-------------Patching-------------");

            var harmony = HarmonyInstance.Create("me.pipe01.pitung");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            IGConsole.Init(); // Making the console available in AfterPatch

            foreach (var mod in ModLoader.GetMods())
            {
                if (mod.ModAssembly == null)
                {
                    MDebug.WriteLine($"[ERROR] {mod.Name} failed to load: couldn't load assembly.");
                    continue;
                }

                if (mod.FrameworkVersion.CompareTo(PiTung.FrameworkVersion) != 0)
                {
                    if (mod.RequireFrameworkVersion)
                    {
                        MDebug.WriteLine($"[ERROR] {mod.Name} failed to load: wrong PiTUNG version.");
                        continue;
                    }
                    else
                    {
                        MDebug.WriteLine($"[WARNING] {mod.Name} may not work properly: wrong PiTUNG version");
                    }
                }

                try
                {
                    mod.BeforePatch();
                }
                catch (Exception ex)
                {
                    MDebug.WriteLine($"[ERROR] {mod.Name} failed to load: error while executing before-patch method.");
                    MDebug.WriteLine("More details: " + ex, 1);

                    continue;
                }

                MDebug.WriteLine($"Loading {mod.FullName}...");

                try
                {
                    harmony.PatchAll(mod.ModAssembly);

                    foreach (Type cls in mod.ModAssembly.GetTypes())
                    {
                        var attrs = (TargetAttribute[])cls.GetCustomAttributes(typeof(TargetAttribute), false);

                        if (attrs.Length == 0)
                            continue;

                        foreach (var patch in PatchUtilities.GetMethodPatches(cls, attrs[0].ContainerType))
                        {
                            var method = new HarmonyMethod(patch.PatchMethod);
                            
                            harmony.Patch(
                                patch.BaseMethod,
                                patch.Prefix ? method : null,
                                patch.Postfix ? method : null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MDebug.WriteLine($"[ERROR] {mod.Name} failed to load: error while patching methods.");
                    MDebug.WriteLine("More details: " + ex, 1);
                    continue;
                }

                try
                {
                    mod.AfterPatch();
                }
                catch (Exception ex)
                {
                    MDebug.WriteLine($"[ERROR] {mod.Name} failed to load: error while executing after-patch method.");
                    MDebug.WriteLine("More details: " + ex, 1);

                    continue;
                }

                ModCount++;
                MDebug.WriteLine($"{mod.Name} loaded successfully.");
            }

            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

            MDebug.WriteLine("----------Done patching!----------");
        }

        /// <summary>
        /// Gets called when the active scene is changed.
        /// </summary>
        /// <param name="arg0">I have no idea.</param>
        /// <param name="arg1">The new scene.</param>
        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            var objs = arg1.GetRootGameObjects();

            //Search for a camera. If we find one, check if it has already got a DummyComponent.
            //If it doesn't, add one.
            foreach (var obj in objs)
            {
                var camera = obj.GetComponent<Camera>();

                if (camera != null)
                {
                    MDebug.WriteLine("Found camera!", 1);

                    if (obj.GetComponent<DummyComponent>() == null)
                        camera.gameObject.AddComponent<DummyComponent>();

                    break;
                }
            }

            //If the scene's name is "main menu", we may might possibly probably be in the main menu.
            ModUtilities.IsOnMainMenu = arg1.name == "main menu";
        }
    }
}
