using PiTung.Console;
using References;
using System;
using UnityEngine;

namespace PiTung.Components
{
    public abstract class CubicComponent : CustomComponent
    {
        protected IOMap IOMap { get; }

        public CubicComponent()
        {
            this.IOMap = new IOMap();
            this.IOMap.Changed += (a, b) => UpdatePrefab();
        }

        protected override GameObject BuildComponentPrefab()
        {
            var prefab = CreatePrefabFromCode(this.IOMap);
            AddScriptToGameObject(prefab);

            return prefab;
        }

        protected abstract void AddScriptToGameObject(GameObject @object);

        private static GameObject CreatePrefabFromCode(IOMap map)
        {
            GameObject PrefabRoot = GameObject.Instantiate(Prefabs.WhiteCube, new Vector3(-1000, -1000, -1000), Quaternion.identity);
            PrefabRoot.transform.localScale = WhiteCubeScale;
            
            foreach (var item in map.Sides)
            {
                AddIO(item.Key, item.Value);
            }
            
            return PrefabRoot;

            void AddIO(CubeSide side, SideType type)
            {
                if (type == SideType.None)
                    return;

                float x = 0, y = 0.5f, z = 0, rotX = 0, rotY = 0, rotZ = 0;

                switch (side)
                {
                    case CubeSide.Top:
                        y = 1f;
                        rotY = 90;
                        break;
                    case CubeSide.Front:
                        z = -0.5f;
                        rotX = 270;
                        break;
                    case CubeSide.Left:
                        x = -0.5f;
                        rotZ = 90;
                        break;
                    case CubeSide.Back:
                        z = 0.5f;
                        rotX = 90;
                        break;
                    case CubeSide.Right:
                        x = 0.5f;
                        rotZ = 270;
                        break;
                }

                var prefab = type == SideType.Input ? Prefabs.Peg : Prefabs.Output;
                GameObject Peg = GameObject.Instantiate(prefab, PrefabRoot.transform);
                Peg.transform.localPosition = new Vector3(x, y, z);
                Peg.transform.localScale = type == SideType.Input ? PegScale : OutputScale;
                Peg.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);

                if (type == SideType.Output)
                {
                    var comp = Peg.GetComponent<CircuitOutput>();
                    comp.On = false;
                    comp.RecalculateCombinedMesh();
                }
            }
        }
    }
}
