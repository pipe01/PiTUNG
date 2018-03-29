using References;
using System;
using UnityEngine;

namespace PiTung.Components
{
    public static class CustomPrefabBuilder
    {
        public static CubeBuilder Cube => new CubeBuilder();

        public static CustomBuilder Custom(GameObject root) => new CustomBuilder(root);
    }


    public abstract class Builder
    {
        internal BuildState State = new BuildState();
    }

    public class CustomBuilder : Builder
    {
        internal CustomBuilder(GameObject root)
        {
            State.Structure = new CustomStructureAtom(root);
        }

        public CustomBuilder AddInput(Vector3 position)
        {
            State.Atoms.Add(new InputPegAtom { Position = position });

            return this;
        }

        public CustomBuilder AddOutput(Vector3 position)
        {
            State.Atoms.Add(new OutputAtom { Position = position });

            return this;
        }
    }

    public class CubeBuilder : Builder
    {
        private IOMapAtom Atom = new IOMapAtom { Map = new IOMap() };

        internal CubeBuilder()
        {
            State.Structure = new CubeStructureAtom();
            State.Atoms.Add(Atom);
        }

        public CubeBuilder SetSide(CubeSide side, SideType what)
        {
            Atom.Map.SetSide(side, what);

            return this;
        }
    }
}
