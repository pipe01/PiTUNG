using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung.Components
{
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
                    _inputs = OrderByIOIndex(this.GetComponentsInChildren<CircuitInput>());
                    UpdateInputParents();
                }

                return _inputs;
            }
            internal set => _inputs = OrderByIOIndex(value);
        }

        private CircuitOutput[] _outputs;
        /// <summary>
        /// The component's outputs. Won't be null if there aren't any.
        /// </summary>
        public CircuitOutput[] Outputs
        {
            get
            {
                return _outputs ?? (_outputs = OrderByIOIndex(this.GetComponentsInChildren<CircuitOutput>()));
            }
            internal set => _outputs = OrderByIOIndex(value);
        }

        internal void UpdateInputParents()
        {
            foreach (var item in Inputs)
            {
                item.CircuitLogicComponent = this;
            }
        }

        private static T[] OrderByIOIndex<T>(T[] e) where T : MonoBehaviour
        {
            return e.OrderBy(o => o.GetComponent<IOInfo>()?.Index ?? -1).ToArray();
        }
    }
}
