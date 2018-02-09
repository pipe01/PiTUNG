using UnityEngine;

namespace PiTung_Bootstrap.Building
{
    /// <summary>
    /// Represents a type of component that can be added to boards. See <see cref="Components"/> to get a list of available components.
    /// </summary>
    public struct CircuitComponent
    {
        /// <summary>
        /// The component's name.
        /// </summary>
        public string Name { get; }

        internal GameObject Prefab { get; }

        internal CircuitComponent(string name, GameObject prefab)
        {
            this.Name = name;
            this.Prefab = prefab;
        }
    }
}
