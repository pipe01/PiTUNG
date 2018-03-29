using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung.Components
{
    public static class ComponentRegistry
    {
        private struct ComponentStruct
        {
            public string Name;
            public Type ComponentType;
            public GameObject Prefab;

            public ComponentStruct(string name, Type componentType, GameObject prefab)
            {
                this.Name = name;
                this.ComponentType = componentType;
                this.Prefab = prefab;
            }
        }

        private static IDictionary<string, ComponentStruct> Registry = new Dictionary<string, ComponentStruct>();

        public static void RegisterComponent<T>() where T : CustomComponent
        {
            var component = Activator.CreateInstance<T>();

            var prefab = component.BuildPrefab();
            MDebug.WriteLine("Register " + prefab);

            RegisterComponent(component.UniqueName, typeof(T), prefab);
        }
        
        internal static void RegisterComponent(string name, Type type, GameObject prefab)
        {
            Registry[name] = new ComponentStruct(name, type, prefab);
        }

        internal static void UpdatePrefab(string name, GameObject prefab)
        {
            if (Registry.TryGetValue(name, out var item))
            {
                Registry[name] = new ComponentStruct(name, item.ComponentType, prefab);
            }
        }

        internal static Type GetTypeFromName(string name)
        {
            if (Registry.TryGetValue(name, out var item))
            {
                return item.ComponentType;
            }

            return null;
        }

        internal static GameObject GetPrefabFromName(string name)
        {
            if (Registry.TryGetValue(name, out var item))
            {
                var comp = (CustomComponent)Activator.CreateInstance(item.ComponentType);

                return comp.BuildPrefab();
            }
            
            return null;
        }
    }
}
