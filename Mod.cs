using System.Linq;
using PiTung_Bootstrap.Config_menu;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PiTung_Bootstrap
{
    public abstract class Mod
    {
        internal static List<Mod> AliveMods => Bootstrapper.Mods;

        protected Mod()
        {
            if (ModKeys != null)
                SubscribeToKeys(ModKeys);
        }
        
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
        /// Setting this property to <see cref="PiTung.FrameworkVersion"/> is strongly discouraged. If you
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
        /// The keys the mod will be notified about. You can alternatively use the <see cref="Mod.SubscribeToKey(KeyCode)"/> and <see cref="Mod.SubscribeToKeys(KeyCode[])"/> methods.
        /// </summary>
        protected virtual KeyCode[] ModKeys { get; } = null;


        /// <summary>
        /// The mod's full name. Format: {Author}'s {Name} (v{ModVersion})
        /// </summary>
        public string FullName => $"{Author}'s {Name} (v{ModVersion})";

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
        public virtual void WorldLoaded(string worldName) { }

        /// <summary>
        /// Equivalent to <see cref="MonoBehaviour.Update"/>
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Equivalent to <see cref="MonoBehaviour.OnGUI"/>
        /// </summary>
        public virtual void OnGUI() { }

        /// <summary>
        /// Called in the frame that a subscribed key has been pushed down.
        /// </summary>
        /// <param name="key">The <see cref="KeyCode"/> of the key that has been pressed.</param>
        public virtual void OnKeyDown(KeyCode key) { }

        /// <summary>
        /// Subscribes to a key. Subscribed keys will raise the <see cref="OnKeyDown(KeyCode)"/> method.
        /// </summary>
        /// <param name="key"></param>
        protected void SubscribeToKey(KeyCode key)
        {
            ModUtilities.Input.SubscribeToKey(key, OnKeyDown);
        }

        /// <summary>
        /// Subscribes to multiple keys at once. See <see cref="SubscribeToKey(KeyCode)"/>.
        /// </summary>
        /// <param name="keys"></param>
        protected void SubscribeToKeys(params KeyCode[] keys)
        {
            foreach (var item in keys)
            {
                SubscribeToKey(item);
            }
        }
    }
}
