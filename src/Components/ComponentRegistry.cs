using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PiTung.Components
{
    /// <summary>
    /// Keeps track of all the custom components.
    /// </summary>
    public static class ComponentRegistry
    {
        internal static IDictionary<string, CustomComponent> Registry = new Dictionary<string, CustomComponent>();

        /// <summary>
        /// Deprecated, use <see cref="CreateNew{THandler}(string, string, Builder)"/>.
        /// </summary>
        [Obsolete("The parameter 'mod' is no longer required.")]
        public static CustomComponent<THandler> CreateNew<THandler>(Mod mod, string name, string displayName, Builder builder) where THandler : UpdateHandler
        {
            if (Registry.TryGetValue(name, out var i) && i == null)
                Registry.Remove(name);

            var comp = new CustomComponent<THandler>(mod, name, displayName, builder.State);
            Add(name, comp);

            return comp;
        }

        /// <summary>
        /// Deprecated, use <see cref="CreateNew(string, string, Builder)"/>.
        /// </summary>
        [Obsolete("The parameter 'mod' is no longer required.")]
        public static CustomComponent CreateNew(Mod mod, string name, string displayName, Builder builder)
        {
            if (Registry.TryGetValue(name, out var i) && i == null)
                Registry.Remove(name);

            var comp = new CustomComponent<EmptyHandler>(mod, name, displayName, builder.State);
            Add(name, comp);

            return comp;
        }


        /// <summary>
        /// Registers a new custom component with an update handler of type <typeparamref name="THandler"/>.
        /// </summary>
        /// <typeparam name="THandler">The update handler type.</typeparam>
        /// <param name="name">The component's "ugly" name. This name will be used to uniquely identify the component, so make sure that no other mods will use it.</param>
        /// <param name="displayName">The name that will be shown in the components menu.</param>
        /// <param name="builder">The builder that you use to create the structure of the component. See <see cref="PrefabBuilder"/>.</param>
        /// <returns>A <see cref="CustomComponent{THandler}"/> instance. You don't need to store this.</returns>
        public static CustomComponent CreateNew<THandler>(string name, string displayName, Builder builder) where THandler : UpdateHandler
        {
            var mod = Bootstrapper.Instance.GetModByAssembly(Assembly.GetCallingAssembly());
            
            if (Registry.TryGetValue(name, out var i) && i == null)
                Registry.Remove(name);

            var comp = new CustomComponent<THandler>(mod, name, displayName, builder.State);
            Add(name, comp);
            
            return comp;
        }

        /// <summary>
        /// Registers a new custom component with no update handler.
        /// </summary>
        /// <param name="mod">The mod that is registering this component.</param>
        /// <param name="name">The component's "ugly" name. This name will be used to uniquely identify the component, so make sure that no other mods will use it.</param>
        /// <param name="displayName">The name that will be shown in the components menu.</param>
        /// <param name="builder">The builder that you use to create the structure of the component. See <see cref="PrefabBuilder"/>.</param>
        /// <returns>A <see cref="CustomComponent"/> instance. You don't need to store this.</returns>
        public static CustomComponent CreateNew(string name, string displayName, Builder builder)
        {
            var mod = Bootstrapper.Instance.GetModByAssembly(Assembly.GetCallingAssembly());
            
            if (Registry.TryGetValue(name, out var i) && i == null)
                Registry.Remove(name);

            var comp = new CustomComponent<EmptyHandler>(mod, name, displayName, builder.State);
            Add(name, comp);

            return comp;
        }

        private static void Add(string name, CustomComponent comp)
        {
            Registry.Add(name, comp);

            Registry = Registry
                .OrderBy(o => o.Value.Mod.Name)
                .ThenBy(o => o.Value.DisplayName)
                .ToDictionary(o => o.Key, o => o.Value);
        }
    }

    internal class EmptyHandler : UpdateHandler
    {
        protected override void CircuitLogicUpdate()
        {
        }
    }
}
