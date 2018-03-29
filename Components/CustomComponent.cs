using References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung.Components
{
    public abstract class CustomComponent
    {
        public string UniqueName { get; }
        protected internal BuildState Build { get; }

        internal CustomComponent(string name, BuildState build)
        {
            this.UniqueName = name;
            this.Build = build;
        }

        public virtual GameObject Instantiate()
        {
            return this.Build.BuildResult();
            //return GameObject.Instantiate(obj, new Vector3(-1000, -1000, -1000), Quaternion.identity);
        }
    }
    public sealed class CustomComponent<THandler> : CustomComponent where THandler : UpdateHandler
    {
        internal CustomComponent(string name, BuildState build) : base(name, build)
        {
        }
        
        public override GameObject Instantiate()
        {
            var obj = base.Instantiate();
            obj.AddComponent<THandler>().Component = this;
            obj.AddComponent<ObjectInfo>().ComponentType = ComponentType.CustomObject;

            return obj;
        }
    }

    public abstract class UpdateHandler : CircuitLogicComponent
    {
        private CustomComponent _component;
        public CustomComponent Component
        {
            get => _component;
            internal set
            {
                _component = value;

                this.Inputs = this.GetComponentsInChildren<CircuitInput>();
                this.Outputs = this.GetComponentsInChildren<CircuitOutput>();
            }
        }

        public CircuitInput[] Inputs { get; private set; } = new CircuitInput[0];
        public CircuitOutput[] Outputs { get; private set; } = new CircuitOutput[0];
    }
}
