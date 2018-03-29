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

            var handler = obj.AddComponent<THandler>();
            handler.Component = this;
            handler.ComponentName = this.UniqueName;

            obj.AddComponent<ObjectInfo>().ComponentType = ComponentType.CustomObject;

            return obj;
        }
    }

    public abstract class UpdateHandler : CircuitLogicComponent
    {
        public CustomComponent Component { get; internal set; }

        [SerializeField]
        public string ComponentName;

        private CircuitInput[] _inputs;
        public CircuitInput[] Inputs
        {
            get
            {
                if (_inputs == null)
                {
                    _inputs = this.GetComponentsInChildren<CircuitInput>();
                }
                return _inputs;
            }
            private set => _inputs = value;
        }

        private CircuitOutput[] _outputs;
        public CircuitOutput[] Outputs
        {
            get
            {
                if (_outputs == null)
                {
                    _outputs = this.GetComponentsInChildren<CircuitOutput>();
                }
                return _outputs;
            }
            private set => _outputs = value;
        }
    }
}
