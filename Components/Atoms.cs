using References;
using System;
using UnityEngine;

namespace PiTung.Components
{
    internal interface IAtom
    {
        void AddToGameObject(GameObject obj);
    }


    internal abstract class IOAtom : IAtom
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public abstract void AddToGameObject(GameObject obj);
    }

    internal sealed class InputPegAtom : IOAtom
    {
        public override void AddToGameObject(GameObject obj)
        {
            BuilderUtils.AddInputPeg(obj, this.Position).transform.localRotation = this.Rotation;
        }
    }

    internal sealed class OutputAtom : IOAtom
    {
        public override void AddToGameObject(GameObject obj)
        {
            BuilderUtils.AddOutputPeg(obj, this.Position).transform.localRotation = this.Rotation;
        }
    }


    internal sealed class IOMapAtom : IAtom
    {
        public IOMap Map { get; set; }

        public void AddToGameObject(GameObject obj)
        {
            BuilderUtils.ApplyIOMap(obj, Map);
        }
    }

    internal sealed class CubeColorAtom : IAtom
    {
        public Color Color { get; set; }

        public void AddToGameObject(GameObject obj)
        {
            obj.GetComponent<MegaMeshComponent>().MaterialType = MaterialType.CircuitBoard;
            obj.GetComponent<Renderer>().material.color = this.Color;
        }
    }

    internal interface IStructureAtom
    {
        GameObject GetRootObject();
    }

    internal sealed class CubeStructureAtom : IStructureAtom
    {
        public GameObject GetRootObject()
        {
            return GameObject.Instantiate(Prefabs.WhiteCube, new Vector3(-1000, -1000, -1000), Quaternion.identity);
        }
    }
    internal sealed class CustomStructureAtom : IStructureAtom
    {
        public Func<GameObject> Root { get; }

        public CustomStructureAtom(Func<GameObject> root)
        {
            this.Root = root;
        }

        public GameObject GetRootObject()
        {
            return this.Root();
        }
    }
}
