using References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung.Components
{
    /// <summary>
    /// Represents a custom component without an update handler.
    /// </summary>
    public class CustomComponent
    {
        /// <summary>
        /// The component's unique name. It should be unique enough so that it doesn't collide with any other mods.
        /// </summary>
        public string UniqueName { get; }

        /// <summary>
        /// The name that will be shown in the component selector.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The mod that registered this component.
        /// </summary>
        internal Mod Mod { get; }

        internal BuildState Build { get; }

        internal CustomComponent(Mod mod, string name, string displayName, BuildState build)
        {
            this.Mod = mod;
            this.UniqueName = name;
            this.DisplayName = displayName;
            this.Build = build;
        }

        /// <summary>
        /// Spawns a new instance of the custom component.
        /// </summary>
        /// <returns>A new instance of the custom component.</returns>
        public virtual GameObject Instantiate() => this.Build.BuildResult();
    }

    /// <summary>
    /// Represents a custom component with an update handler of type <typeparamref name="THandler"/>.
    /// </summary>
    /// <typeparam name="THandler">The update handler class for this component.</typeparam>
    public sealed class CustomComponent<THandler> : CustomComponent where THandler : UpdateHandler
    {
        internal CustomComponent(Mod mod, string name, string displayName, BuildState build) : base(mod, name, displayName, build)
        {
        }

        /// <summary>
        /// Spawns a new instance of the custom component.
        /// </summary>
        /// <returns>A new instance of the custom component.</returns>
        public override GameObject Instantiate()
        {
            var obj = base.Instantiate();

            var handler = obj.AddComponent<THandler>();
            handler.Component = this;
            handler.ComponentName = this.UniqueName;

            handler.Inputs = obj.GetComponentsInChildren<CircuitInput>();
            handler.Outputs = obj.GetComponentsInChildren<CircuitOutput>();
            handler.UpdateInputParents();

            obj.AddComponent<ObjectInfo>().ComponentType = ComponentType.CustomObject;

            return obj;
        }
    }
}
