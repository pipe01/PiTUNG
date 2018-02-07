using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiTung_Bootstrap.Building
{
    public struct CircuitComponent
    {
        public string Name { get; }
        internal GameObject Prefab { get; }

        internal CircuitComponent(string name, GameObject prefab)
        {
            this.Name = name;
            this.Prefab = prefab;
        }
    }
}
