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

        internal BuildState Build { get; }

        internal CustomComponent(string name, string displayName, BuildState build)
        {
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
        internal CustomComponent(string name, string displayName, BuildState build) : base(name, displayName, build)
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

    /// <summary>
    /// This class will allow you to handle a component's logic update cycle.
    /// </summary>
    public abstract class UpdateHandler : CircuitLogicComponent
    {
        /// <summary>
        /// The <see cref="CustomComponent"/> class that this handler belongs to.
        /// </summary>
        public CustomComponent Component { get; internal set; }

        internal string ComponentName;
        
        private CircuitInput[] _inputs;
        /// <summary>
        /// The component's inputs. Won't be null if there aren't any.
        /// </summary>
        public CircuitInput[] Inputs
        {
            get
            {
                if (_inputs == null)
                {
                    _inputs = this.GetComponentsInChildren<CircuitInput>();
                    UpdateInputParents();
                }

                return _inputs;
            }
            internal set => _inputs = value;
        }

        private CircuitOutput[] _outputs;
        /// <summary>
        /// The component's outputs. Won't be null if there aren't any.
        /// </summary>
        public CircuitOutput[] Outputs
        {
            get
            {
                return _outputs ?? (_outputs = this.GetComponentsInChildren<CircuitOutput>());
            }
            internal set => _outputs = value;
        }

        internal void UpdateInputParents()
        {
            foreach (var item in Inputs)
            {
                item.CircuitLogicComponent = this;
            }
        }
    }
}
