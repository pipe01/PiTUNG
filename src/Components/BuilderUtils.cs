using References;
using UnityEngine;

namespace PiTung.Components
{
    internal static class BuilderUtils
    {
        internal static readonly Vector3 PegScale = new Vector3(0.3f, 0.8f, 0.3f);
        internal static readonly Vector3 OutputScale = new Vector3(0.5f, 0.4f, 0.5f);
        internal static readonly Vector3 WhiteCubeScale = new Vector3(0.3f, 0.3f, 0.3f);

        public static void ApplyIOMap(GameObject prefabRoot, IOMap map)
        {
            foreach (var item in map.Sides)
            {
                AddIO(item.Key, item.Value);
            }
            
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
                GameObject Peg = GameObject.Instantiate(prefab, prefabRoot.transform);
                Peg.transform.localPosition = new Vector3(x, y, z);
                Peg.transform.localScale = type == SideType.Input ? PegScale : OutputScale;
                Peg.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);

                if (type == SideType.Output)
                {
                    var comp = Peg.GetComponent<CircuitOutput>();
                    comp.On = false;
                    comp.RecalculateCombinedMesh();
                }

                Peg.AddComponent<IODummyComponent>().Side = side;
            }
        }

        public static GameObject AddInputPeg(GameObject parent, Vector3? localPosition = null)
        {
            var Peg = GameObject.Instantiate(Prefabs.Peg);

            Peg.transform.localScale = new Vector3(PegScale.x * 0.3f, PegScale.y * 0.3f, PegScale.z * 0.3f);
            Peg.transform.parent = parent.transform;

            if (localPosition != null)
                Peg.transform.localPosition = localPosition.Value;

            return Peg;
        }

        public static GameObject AddOutputPeg(GameObject parent, Vector3? localPosition = null)
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
}
