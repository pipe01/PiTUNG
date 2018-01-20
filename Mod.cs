using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PiTung_Bootstrap
{
    public abstract class Mod
    {
        internal static List<Mod> AliveMods = new List<Mod>();

        public Mod()
        {
            AliveMods.Add(this);

            if (ModKeys != null)
                SubscribeToKeys(ModKeys);
        }

        ~Mod()
        {
            AliveMods.Remove(this);
        }

        internal Assembly ModAssembly { get; set; }

        /// <summary>
        /// The absolute path to the mod's DLL file.
        /// </summary>
        public string FullPath { get; internal set; }
        
        public abstract string ModName { get; }
        public abstract string ModAuthor { get; }
        public abstract Version ModVersion { get; }

        /// <summary>
        /// The keys the mod will be notified about. You can alternatively use the <see cref="Mod.SubscribeToKey(KeyCode)"/> and <see cref="Mod.SubscribeToKeys(KeyCode[])"/> methods.
        /// </summary>
        protected virtual KeyCode[] ModKeys { get; } = null;

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
        public virtual void LodingWorld(string worldName) { }

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

        /// <summary>
        /// Goes through all the mod's methods and returns <see cref="MethodPatch"/>es defining the mod's patches.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<MethodPatch> GetMethodPatches()
        {
            foreach (var item in this.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                var attrs = Attribute.GetCustomAttributes(item);

                foreach (var a in attrs)
                {
                    bool prefix = a is PrefixAttribute;
                    bool postfix = a is PostfixAttribute;
                    PatchAttribute patch = a as PatchAttribute;
                    
                    if (!prefix && !postfix)
                        continue;

                    var baseMethod = patch.ContainerType.GetMethod(patch.MethodName, BindingFlags.NonPublic | BindingFlags.Instance);

                    if (baseMethod == null)
                    {
                        throw new ArgumentException($"Can't find method {patch.MethodName} in {patch.ContainerType.Name}");
                    }

                    yield return new MethodPatch(baseMethod, item, prefix, postfix);
                    break;
                }
            }
        }
    }
}
