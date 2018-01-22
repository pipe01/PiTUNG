using Harmony;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

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

            foreach (var mod in ModLoader.GetMods())
            {
                string modStr = $"Mod \"{mod.FullName}\"";

                if (mod.ModAssembly == null)
                {
                    MDebug.WriteLine($"[ERROR] {modStr} failed to load: couldn't load assembly.");
                    continue;
                }

                if (mod.FrameworkVersion != PiTung.FrameworkVersion)
                {
                    if (mod.RequireFrameworkVersion)
                    {
                        MDebug.WriteLine($"[ERROR] {modStr} failed to load: wrong PiTUNG version.");
                        continue;
                    }
                    else
                    {
                        MDebug.WriteLine($"[WARNING] {modStr} may not work properly: wrong PiTUNG version");
                    }
                }

                try
                {
                    mod.BeforePatch();
                }
                catch (Exception ex)
                {
                    MDebug.WriteLine($"[ERROR] {modStr} failed to load: error while executing before-patch method.");
                    MDebug.WriteLine("More details: " + ex.Message, 1);

                    continue;
                }

                try
                {
                    harmony.PatchAll(mod.ModAssembly);

                    foreach (var patch in mod.GetMethodPatches())
                    {
                        if (patch.Prefix)
                        {
                            harmony.Patch(patch.BaseMethod, new HarmonyMethod(patch.PatchMethod), null);
                        }
                        else if (patch.Postfix)
                        {
                            harmony.Patch(patch.BaseMethod, null, new HarmonyMethod(patch.PatchMethod));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MDebug.WriteLine($"[ERROR] {modStr} failed to load: error while patching methods.");
                    MDebug.WriteLine("More details: " + ex.Message, 1);
                    continue;
                }

                try
                {
                    mod.AfterPatch();
                }
                catch (Exception ex)
                {
                    MDebug.WriteLine($"[ERROR] {modStr} failed to load: error while executing after-patch method.");
                    MDebug.WriteLine("More details: " + ex.Message, 1);

                    continue;
                }

                ModCount++;
                MDebug.WriteLine($"'{mod.FullName}' loaded successfully.");
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
