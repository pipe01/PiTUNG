using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiTung_Bootstrap.Building
{
    /// <summary>
    /// Manages the game's available circuit components.
    /// </summary>
    public static class Components
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

        internal static IEnumerable<string> GetComponentNames() => Prefabs.Keys;

        /// <summary>
        /// Gets a component called <paramref name="name"/>
        /// </summary>
        /// <param name="name">The component's name.</param>
        /// <returns>A <see cref="CircuitComponent"/> representing the component type.</returns>
        public static CircuitComponent GetComponent(string name)
        {
            if (Prefabs.TryGetValue(name, out var v))
                return new CircuitComponent(v.name, v);

            throw new ArgumentException($"Invalid component '{name}'.", nameof(name));
        }

        /// <summary>
        /// Gets all the available components.
        /// </summary>
        /// <returns>A list with all available components.</returns>
        public static IEnumerable<CircuitComponent> GetComponents()
        {
            return Prefabs.Select(o => new CircuitComponent(o.Value.name, o.Value));
        }
    }
}
