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
        internal static IDictionary<string, CustomComponent> Registry = new Dictionary<string, CustomComponent>();

        public static CustomComponent<THandler> CreateNew<THandler>(Mod mod, string name, string displayName, Builder builder) where THandler : UpdateHandler
        {
            if (Registry.TryGetValue(name, out var i) && i == null)
                Registry.Remove(name);

            var comp = new CustomComponent<THandler>(mod, name, displayName, builder.State);
            Registry.Add(name, comp);

            return comp;
        }

        public static CustomComponent CreateNew(Mod mod, string name, string displayName, Builder builder)
        {
            if (Registry.TryGetValue(name, out var i) && i == null)
                Registry.Remove(name);

            var comp = new CustomComponent<EmptyHandler>(mod, name, displayName, builder.State);
            Registry.Add(name, comp);

            return comp;
        }
    }

    internal class EmptyHandler : UpdateHandler
    {
        protected override void CircuitLogicUpdate()
        {
        }
    }
}
