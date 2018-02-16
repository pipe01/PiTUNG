using TMPro.Examples;
using System.Security.AccessControl;
using PiTung.Console;
using System.Linq;
using PiTung.Config_menu;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PiTung
{
    /// <summary>
    /// Indicates the precision used when comparing PiTUNG versions.
    /// </summary>
    public enum VersionPrecision
    {
        /// <summary>
        /// Matches if the major and minor numbers are equal.
        /// </summary>
        Minor,

        /// <summary>
        /// Matches if the major, minor and build numbers are equal.
        /// </summary>
        Build
    }

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
        /// Indicates what numbers to include when comparing PiTUNG versions. Defaults to <see cref="VersionPrecision.Minor"/>.
        /// </summary>
        public virtual VersionPrecision MatchVersionUpTo { get; }

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
        /// Returns the version of PiTUNG that this mod was compiled with.
        /// </summary>
        internal Version CompiledWithVersion
        {
            get
            {
                var asses = this.GetType().Assembly.GetReferencedAssemblies();
                
                return asses.SingleOrDefault(o => o.Name.Contains("PiTung")).Version;
            }
        }

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

        internal bool MatchesVersion()
        {
            bool ret = true;
            Version v1 = this.CompiledWithVersion;
            Version v2 = PiTUNG.FrameworkVersion;

            if (ret && v1.Major != v2.Major)
                ret = false;

            if (ret && v1.Minor != v2.Minor && (MatchVersionUpTo == VersionPrecision.Minor || MatchVersionUpTo == VersionPrecision.Build))
                ret = false;

            if (ret && v1.Build != v2.Build && MatchVersionUpTo == VersionPrecision.Build)
                ret = false;

            return ret;
        }

        internal Version GetRequiredVersion()
        {
            var ver = CompiledWithVersion;

            if (MatchVersionUpTo == VersionPrecision.Minor)
                return new Version(ver.Major, ver.Minor);
            else if (MatchVersionUpTo == VersionPrecision.Build)
                return new Version(ver.Major, ver.Minor, ver.Build);

            throw new Exception("aaaaa");
        }
    }
}
