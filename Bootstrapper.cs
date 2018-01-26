using System.Collections.Generic;
using System.Linq;
using PiTung_Bootstrap.Config_menu;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using PiTung_Bootstrap.Console;
using Object = UnityEngine.Object;

namespace PiTung_Bootstrap
{
    public class Bootstrapper
    {
        private static bool Patched = false;
        private static List<string> LoadedMods = new List<string>();
        private static HarmonyInstance _Harmony;

        /// <summary>
        /// The <see cref="Bootstrapper"/> singleton instance.
        /// </summary>
        public static Bootstrapper Instance { get; } = new Bootstrapper();

        private Bootstrapper()
        {
        }
        
        /// <summary>
        /// How many mods are currently loaded.
        /// </summary>
        public static int ModCount => LoadedMods.Count;

        /// <summary>
        /// Main bootstrap method. Loads and patches all mods.
        /// </summary>
        public void Patch(bool hotload = false)
        {
            if (Patched && !hotload)
                return;
            Patched = true;

            MDebug.WriteLine("PiTUNG Framework version {0}", 0, PiTung.FrameworkVersion);
            MDebug.WriteLine("-------------Patching-------------" + (hotload ? " (reloading)" : ""));

            if (!hotload)
            {
                LoadedMods.Clear();

                _Harmony = HarmonyInstance.Create("me.pipe01.pitung");
                _Harmony.PatchAll(Assembly.GetExecutingAssembly());

                IGConsole.Init();

                SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            }

            AddDummyComponent(SceneManager.GetActiveScene());

            foreach (var mod in ModLoader.GetMods())
            {
                if (LoadedMods.Contains(mod.FullPath))
                {
                    MDebug.WriteLine($"Skipping already loaded mod {mod.Name}.");
                    continue;
                }

                if (!mod.Hotloadable && hotload)
                {
                    MDebug.WriteLine($"[WARNING] {mod.Name} can't be hotloaded.");
                    continue;
                }

                if (mod.ModAssembly == null)
                {
                    MDebug.WriteLine($"[ERROR] {mod.Name} failed to load: couldn't load assembly.");
                    continue;
                }

                if (!mod.FrameworkVersion.EqualsVersion(PiTung.FrameworkVersion))
                {
                    if (mod.RequireFrameworkVersion)
                    {
                        MDebug.WriteLine($"[ERROR] {mod.Name} failed to load: wrong PiTUNG version. Required version: {mod.FrameworkVersion}.");
                        continue;
                    }
                    else
                    {
                        MDebug.WriteLine($"[WARNING] {mod.Name} may not work properly: wrong PiTUNG version. Optimal version: {mod.FrameworkVersion}.");
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
                    _Harmony.PatchAll(mod.ModAssembly);

                    foreach (Type cls in mod.ModAssembly.GetTypes())
                    {
                        var attrs = (TargetAttribute[])cls.GetCustomAttributes(typeof(TargetAttribute), false);

                        if (attrs.Length == 0)
                            continue;

                        foreach (var patch in PatchUtilities.GetMethodPatches(cls, attrs[0].ContainerType))
                        {
                            var method = new HarmonyMethod(patch.PatchMethod);
                            
                            _Harmony.Patch(
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

                MenuEntry[] entries;

                try
                {
                    entries = mod.GetMenuEntries().ToArray();
                }
                catch (Exception)
                {
                    MDebug.WriteLine($"[ERROR] {mod.Name} failed to load: error while creating menu entries.");

                    continue;
                }

                if (entries.Length > 0)
                {
                    var entry = new TextMenuEntry { Text = mod.Name };
                    entry.AddChildren(entries);

                    ConfigMenu.Instance.Entries.Add(entry);
                }

                LoadedMods.Add(mod.FullPath);
                MDebug.WriteLine($"{mod.Name} loaded successfully.");
            }

            MDebug.WriteLine("----------Done patching!----------");

            UpdateChecker.UpdateStatus += (a, v) =>
            {
                if (a)
                    IGConsole.Log($"<color=#00ff00>PiTUNG version {v} available!</color> Run Installer.exe to update.");
            };
            ModUtilities.DummyComponent.StartCoroutine(UpdateChecker.CheckUpdates());
        }

        /// <summary>
        /// Gets called when the active scene is changed.
        /// </summary>
        /// <param name="arg0">I have no idea.</param>
        /// <param name="arg1">The new scene.</param>
        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            AddDummyComponent(arg1);
            
            //If the scene's name is "main menu", we may might possibly probably be in the main menu.
            ModUtilities.IsOnMainMenu = arg1.name == "main menu";
        }

        private void AddDummyComponent(Scene scene)
        {
            ModUtilities.DummyComponent = Object.FindObjectOfType<DummyComponent>();

            var objs = scene.GetRootGameObjects();

            if (ModUtilities.DummyComponent == null)
            {
                //Search for a camera. If we find one, check if it has already got a DummyComponent.
                //If it doesn't, add one.
                foreach (var obj in objs)
                {
                    var camera = obj.GetComponent<Camera>();

                    if (camera != null)
                    {
                        ModUtilities.DummyComponent = obj.AddComponent<DummyComponent>();

                        break;
                    }
                }
            }
        }
    }
}
