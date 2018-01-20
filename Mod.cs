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

        public string FullPath { get; internal set; }
        
        internal Assembly ModAssembly { get; set; }

        public abstract string ModName { get; }
        public abstract string ModAuthor { get; }
        public abstract Version ModVersion { get; }

        protected virtual KeyCode[] ModKeys { get; } = null;

        public virtual void BeforePatch() { }
        public virtual void AfterPatch() { }

        public virtual void LodingWorld(string worldName) { }
        public virtual void Update() { }
        public virtual void OnGUI() { }

        public virtual void OnKeyDown(KeyCode key) { }


        protected void SubscribeToKey(KeyCode key)
        {
            ModUtilities.Input.SubscribeToKey(key, OnKeyDown);
        }

        protected void SubscribeToKeys(params KeyCode[] keys)
        {
            foreach (var item in keys)
            {
                SubscribeToKey(item);
            }
        }

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
