using System.Threading;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using PiTung.Config_menu;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using PiTung.Console;
using Object = UnityEngine.Object;
using System.Text.RegularExpressions;

namespace PiTung
{
    /// <summary>
    /// Main patcher, gets called when the game starts and begins the patching process.
    /// </summary>
    public class Bootstrapper
    {
        private static bool Patched = false;
        private static HarmonyInstance _Harmony;

        /// <summary>
        /// Must be set before calling <see cref="Patch(bool)"/>.
        /// </summary>
        public bool Testing { get; set; } = false;

        /// <summary>
        /// True if any loaded mod has available updates.
        /// </summary>
        public bool ModUpdatesAvailable { get; internal set; } = false;

        /// <summary>
        /// The <see cref="Bootstrapper"/> singleton instance.
        /// </summary>
        public static Bootstrapper Instance { get; } = new Bootstrapper();

        internal static List<Mod> _Mods = new List<Mod>();
        /// <summary>
        /// All successfully loaded mods.
        /// </summary>
        public static IList<Mod> Mods => new ReadOnlyCollection<Mod>(_Mods);

        /// <summary>
        /// How many mods are currently loaded.
        /// </summary>
        public static int ModCount => _Mods.Count;

        internal static Mod CurrentlyLoading { get; private set; }

        /// <summary>
        /// List of mods that require a different PiTUNG version and haven't been loaded.
        /// </summary>
        private List<Mod> CheckUpdatesBeforeLoading = new List<Mod>();

        private Bootstrapper()
        {
        }
        
        /// <summary>
        /// Main bootstrap method. Loads and patches all mods.
        /// </summary>
        public void Patch(bool hotload = false)
        {
            if (Patched && !hotload)
                return;
            Patched = true;
            
            string tungVersion = GetTungVersion();

            MDebug.WriteLine("PiTUNG Framework version {0} on TUNG {1}", 0, new Version(PiTUNG.FrameworkVersion.Major, PiTUNG.FrameworkVersion.Minor, PiTUNG.FrameworkVersion.Build), tungVersion);
            MDebug.WriteLine("-------------Patching-------------" + (hotload ? " (reloading)" : ""));

            if (!hotload)
            {
                _Mods.Clear();

                _Harmony = HarmonyInstance.Create("me.pipe01.pitung");

                if (!Testing)
                {
                    try
                    {
                        _Harmony.PatchAll(Assembly.GetExecutingAssembly());
                    }
                    catch (Exception ex)
                    {
                        MDebug.WriteLine("[ERROR] PiTUNG failed to load! Exception: \n" + ex);
                        return;
                    }

                    ModInput.LoadBinds();
                    IGConsole.Init();
                }

                SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            }

            if (!Testing)
                AddDummyComponent(SceneManager.GetActiveScene());

            SelectionMenu.AllowModdedComponents = true;

            new Thread(() => PatchThread(hotload)).Start();
        }

        private string GetTungVersion()
        {
            var obj = GameObject.Find("Version Number");
            var str = obj.GetTextMeshProUGUIText();
            
            return Regex.Match(str, @"v(.\..\..)").Groups[1].Value;
        }

        private void PatchThread(bool hotload)
        {
            foreach (var mod in ModLoader.GetMods())
            {
                CurrentlyLoading = mod;
                LoadMod(mod, hotload);
            }
            CurrentlyLoading = null;

            MDebug.WriteLine("----------Done patching!----------");

            if (hotload)
                return;

            UpdateChecker.UpdateStatus += (a, v) =>
            {
                if (a)
                    IGConsole.Log($"<color=#00ff00>PiTUNG version {v} available!</color> Run Installer.exe to update.");
            };
            ModUtilities.DummyComponent?.StartCoroutine(UpdateChecker.CheckUpdates());

            foreach (var item in Mods.Concat(CheckUpdatesBeforeLoading))
            {
                ModUtilities.DummyComponent?.StartCoroutine(ModUpdater.CheckUpdatesForMod(item, true));
            }
        }

        internal void LoadMod(Mod mod, bool hotload)
        {
            if (_Mods.Any(o => o.FullPath.Equals(mod.FullPath)))
            {
                MDebug.WriteLine($"Skipping already loaded mod {mod.Name}.");
                return;
            }

            if (!mod.Hotloadable && hotload)
            {
                MDebug.WriteLine($"[WARNING] Skipping {mod.Name}: can't be hotloaded.");
                return;
            }

            if (mod.ModAssembly == null)
            {
                LoadError($"{mod.Name} failed to load: couldn't load assembly.", mod.Name);
                return;
            }
            
            if (!mod.MatchesVersion())
            {
                LoadError($"{mod.Name} failed to load: wrong PiTUNG version. Required version: " + mod.GetRequiredVersion(), mod.Name);

                return;
            }

            try
            {
                mod.BeforePatch();
            }
            catch (Exception ex)
            {
                LoadError($"{mod.Name} failed to load: error while executing before-patch method.", mod.Name);
                MDebug.WriteLine("More details: " + ex, 1);

                return;
            }
            
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
                LoadError($"{mod.Name} failed to load: error while patching methods.", mod.Name);
                MDebug.WriteLine("More details: " + ex, 1);

                return;
            }

            try
            {
                mod.AfterPatch();
            }
            catch (Exception ex)
            {
                LoadError($"{mod.Name} failed to load: error while executing after-patch method.", mod.Name);
                MDebug.WriteLine("More details: " + ex, 1);

                return;
            }

            MenuEntry[] entries;

            try
            {
                entries = mod.GetMenuEntries().ToArray();
            }
            catch (Exception)
            {
                LoadError($"{mod.Name} failed to load: error while creating menu entries.", mod.Name);

                return;
            }

            if (entries.Length > 0)
            {
                var entry = new TextMenuEntry { Text = mod.Name };
                entry.AddChildren(entries);

                ConfigMenu.Instance.Entries.Add(entry);
            }

            _Mods.Add(mod);
            MDebug.WriteLine($"{mod.Name} loaded successfully.");
        }

        private void LoadError(string str, string mod)
        {
            IGConsole.Error($"Failed to load mod {mod}.");
            MDebug.WriteLine("[ERROR] " + str);
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

        internal Mod GetModByAssembly(Assembly ass)
        {
            var mod = Bootstrapper._Mods.SingleOrDefault(o => o.ModAssembly.FullName.Equals(ass.FullName));

            if (mod == null)
                mod = Bootstrapper.CurrentlyLoading;

            if (mod == null || !mod.ModAssembly.FullName.Equals(ass.FullName))
                return null;

            return mod;
        }
    }
}
