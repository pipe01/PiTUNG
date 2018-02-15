using System.Linq;
using PiTung.Config_menu;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PiTung
{
    /// <summary>
    /// Base class for all mods.
    /// </summary>
    public abstract partial class Mod
    {
        internal Assembly ModAssembly { get; set; }

        /// <summary>
        /// The absolute path to the mod's DLL file.
        /// </summary>
        public string FullPath { get; internal set; }

        /// <summary>
        /// The mod's name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The mod's package name, e.g. 'me.pipe01.MyMod'.
        /// </summary>
        public abstract string PackageName { get; }

        /// <summary>
        /// Your name.
        /// </summary>
        public abstract string Author { get; }

        /// <summary>
        /// The mod's version.
        /// </summary>
        public abstract Version ModVersion { get; }

        /// <summary>
        /// The version of PiTUNG this mod is using. If a version number is not included, it is considered
        /// a wild card. E.g, if this property equals "1.0", the mod will work on PiTUNG version "1.0.123", but not
        /// if this property equals "1.0.0".
        /// <para/>
        /// Setting this property to <see cref="PiTUNG.FrameworkVersion"/> is strongly discouraged. If you
        /// need your mod to be able to be loaded in any PiTUNG version, set <see cref="RequireFrameworkVersion"/>
        /// to true.
        /// </summary>
        public abstract Version FrameworkVersion { get; }
        
        /// <summary>
        /// If false, the mod will be loaded even when being loaded in a different framework version.
        /// </summary>
        public virtual bool RequireFrameworkVersion { get; } = true;

        /// <summary>
        /// If true, the mod will be able to be loaded without needing to restart the game.
        /// </summary>
        public virtual bool Hotloadable { get; } = false;

        /// <summary>
        /// The update manifest's URL. Defaults to null (disabled).
        /// </summary>
        public virtual string UpdateUrl { get; } = null;

        /// <summary>
        /// If true, there is an update available for this mod.
        /// </summary>
        internal bool HasAvailableUpdate { get; set; } = false;
        
        /// <summary>
        /// The mod's full name. Format: {Author}'s {Name} (v{ModVersion})
        /// </summary>
        public string FullName => $"{Author}'s {Name} (v{ModVersion})";

        #region Mod events
        /// <summary>
        /// Get the mod's menu entries.
        /// </summary>
        public virtual IEnumerable<MenuEntry> GetMenuEntries() => Enumerable.Empty<MenuEntry>();

        /// <summary>
        /// Executed before the mod's patches are applied. Use this to initialize any variables you need.
        /// </summary>
        public virtual void BeforePatch() { }

        /// <summary>
        /// Executed after the mod's patches are applied.
        /// </summary>
        public virtual void AfterPatch() { }

        /// <summary>
        /// Called when a world has been loaded.
        /// </summary>
        /// <param name="worldName">The loaded world's name.</param>
        public virtual void OnWorldLoaded(string worldName) { }

        /// <summary>
        /// Equivalent to MonoBehaviour.Update.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Equivalent to MonoBehaviour.OnGUI.
        /// </summary>
        public virtual void OnGUI() { }
        #endregion
    }

    public partial class Mod
    {
        internal static IList<Mod> AliveMods => Bootstrapper.Mods;

        internal static void CallOnAllMods(Action<Mod> action)
        {
            foreach (var item in AliveMods)
            {
                action(item);
            }
        }
    }
}
