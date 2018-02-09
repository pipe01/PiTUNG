using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiTung_Bootstrap.Building
{
    internal static class Components
    {
        private static Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();

        internal static void AddComponents(List<GameObject> prefabs)
        {
            Prefabs.Clear();
            foreach (var item in prefabs)
            {
                Prefabs.Add(item.name, item);
            }
        }

        public static IEnumerable<string> GetComponentNames() => Prefabs.Keys;

        public static CircuitComponent GetComponent(string name)
        {
            if (Prefabs.TryGetValue(name, out var v))
                return new CircuitComponent(v.name, v);

            throw new ArgumentException($"Invalid component '{name}'.", nameof(name));
        }

        public static IEnumerable<CircuitComponent> GetComponents()
        {
            return Prefabs.Select(o => new CircuitComponent(o.Value.name, o.Value));
        }
    }
}
