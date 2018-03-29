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
        internal static readonly Vector3 PegScale = new Vector3(0.3f, 0.8f, 0.3f);
        internal static readonly Vector3 OutputScale = new Vector3(0.5f, 0.4f, 0.5f);
        internal static readonly Vector3 WhiteCubeScale = new Vector3(0.3f, 0.3f, 0.3f);

        private GameObject PrefabCache;

        public GameObject CreateInstance()
        {
            var prefab = PrefabCache ?? (PrefabCache = UpdatePrefab());

            var info = prefab.AddComponent<ObjectInfo>();
            info.ComponentType = ComponentType.CustomObject;

            foreach (var item in prefab.GetComponents<UpdateScript>())
            {
                item.enabled = true;
            }

            return prefab;
        }
        
        protected abstract GameObject BuildComponentPrefab();
        
        public GameObject BuildPrefab()
        {
            var prefab = BuildComponentPrefab();

            foreach (var item in prefab.GetComponents<UpdateScript>())
            {
                item.Component = this;
                item.UpdateIOs();
            }

            return prefab;
        }

        public abstract string UniqueName { get; }
        
        protected GameObject AddInputPeg(GameObject parent, Vector3? localPosition = null)
        {
            var Peg = GameObject.Instantiate(Prefabs.Peg);

            Peg.transform.localScale = new Vector3(PegScale.x * 0.3f, PegScale.y * 0.3f, PegScale.z * 0.3f);
            Peg.transform.parent = parent.transform;

            if (localPosition != null)
                Peg.transform.localPosition = localPosition.Value;

            return Peg;
        }

        protected GameObject AddOutputPeg(GameObject parent, Vector3? localPosition = null)
        {
            var Peg = GameObject.Instantiate(Prefabs.Output);

            Peg.transform.localScale = new Vector3(OutputScale.x * 0.3f, OutputScale.y * 0.3f, OutputScale.z * 0.3f);
            Peg.transform.parent = parent.transform;

            if (localPosition != null)
                Peg.transform.localPosition = localPosition.Value;
            
            var comp = Peg.GetComponent<CircuitOutput>();
            comp.On = false;
            comp.RecalculateCombinedMesh();

            return Peg;
        }
    }

    public abstract class UpdateScript : CircuitLogicComponent
    {
        internal CustomComponent Component;

        protected CircuitInput[] Inputs { get; private set; }
        protected CircuitOutput[] Outputs { get; private set; }

        protected abstract bool ShouldUpdate();

        protected override void OnAwake()
        {
            this.enabled = false;
        }

        internal void UpdateIOs()
        {
            this.Inputs = GetComponents<CircuitInput>();
            this.Outputs = GetComponents<CircuitOutput>();

            MDebug.WriteLine("INPUTS: " + Inputs.Length);
            MDebug.WriteLine("OUTPUTS: " + Outputs.Length);
        }

        void Update()
        {
            if (this.ShouldUpdate())
            {
                BehaviorManager.UpdatingCircuitLogicComponents.Add(this);
            }
        }
    }
}
