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
        public GameObject Prefab => PrefabCache ?? (PrefabCache = UpdatePrefab());

        protected CircuitInput[] Inputs { get; private set; } = new CircuitInput[0];
        protected CircuitOutput[] Outputs { get; private set; } = new CircuitOutput[0];

        public abstract GameObject BuildPrefab();
        public abstract void LogicUpdate();

        protected virtual bool ShouldUpdate() => false;

        
        protected GameObject UpdatePrefab()
        {
            if (this.PrefabCache != null)
            {
                GameObject.Destroy(this.PrefabCache.GetComponent<UpdateScript>());
                GameObject.Destroy(this.PrefabCache);
            }

            this.PrefabCache = BuildPrefab();

            var updater = this.PrefabCache.AddComponent<UpdateScript>();
            updater.Component = this;

            this.Inputs = this.PrefabCache.GetComponentsInChildren<CircuitInput>();
            this.Outputs = this.PrefabCache.GetComponentsInChildren<CircuitOutput>();

            return this.PrefabCache;
        }
        
        private void Update()
        {
            if (PrefabCache == null)
                UpdatePrefab();

            this.LogicUpdate();
        }

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

        public static GameObject CreateCustomComponent<TComponent>() where TComponent : CustomComponent
        {
            return Activator.CreateInstance<TComponent>().Prefab;
        }

        private class UpdateScript : CircuitLogicComponent
        {
            public CustomComponent Component;

            void Update()
            {
                if (Component?.ShouldUpdate() ?? false)
                {
                    BehaviorManager.UpdatingCircuitLogicComponents.Add(this);
                }
            }

            protected override void CircuitLogicUpdate()
            {
                this.Component?.Update();
            }
        }
    }
}
