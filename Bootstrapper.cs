using Harmony;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PiTung_Bootstrap
{
    public class Bootstrapper
    {
        private static bool Patched = false;

        public static int ModCount = 0;

        public void Patch()
        {
            if (Patched)
                return;
            Patched = true;
            ModCount = 0;

            MDebug.WriteLine("Booting up...");
            
            var harmony = HarmonyInstance.Create("me.pipe01.pitung");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            var mods = ModLoader.GetMods();

            foreach (var mod in mods)
            {
                if (mod.ModAssembly == null)
                {
                    MDebug.WriteLine($"[ERROR] Mod '{mod.ModName}' failed to load: couldn't load assembly.");
                    continue;
                }

                mod.BeforePatch();

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


                mod.AfterPatch();
                
                ModCount++;
                MDebug.WriteLine($"'{mod.ModName}' loaded successfully.");
            }
            
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

            MDebug.WriteLine("Patched successfully!");
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            var objs = arg1.GetRootGameObjects();
            
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

            ModUtilities.IsOnMainMenu = arg1.name == "main menu";
        }
    }
}
