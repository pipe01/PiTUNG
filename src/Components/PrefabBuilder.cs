using References;
using System;
using UnityEngine;

namespace PiTung.Components
{
    /// <summary>
    /// Represents the first step on building a custom component prefab.
    /// </summary>
    public static class PrefabBuilder
    {
        /// <summary>
        /// Returns a cube builder.
        /// </summary>
        public static CubeBuilder Cube => new CubeBuilder();

        /// <summary>
        /// Returns a custom GameObject builder.
        /// </summary>
        /// <param name="root">The function that returns the game object.</param>
        public static CustomBuilder Custom(Func<GameObject> root) => new CustomBuilder(root);
    }
    
    public abstract class Builder
    {
        internal BuildState State = new BuildState();

        public Builder WithComponent<T>()
        {
            State.Components.Add(typeof(T));

            return this;
        }
    }

    public class CustomBuilder : Builder
    {
        internal CustomBuilder(Func<GameObject> root)
        {
            State.Structure = new CustomStructureAtom(root);
        }

        public CustomBuilder WithInput(float x, float y, float z)
        {
            return WithInput(new Vector3(x, y, z));
        }
        public CustomBuilder WithInput(Vector3 position)
        {
            State.Atoms.Add(new InputPegAtom { Position = position });

            return this;
        }
        public CustomBuilder WithInput(Vector3 position, Quaternion rotation)
        {
            State.Atoms.Add(new InputPegAtom { Position = position, Rotation = rotation });

            return this;
        }

        public CustomBuilder WithOutput(float x, float y, float z)
        {
            return WithOutput(new Vector3(x, y, z));
        }
        public CustomBuilder WithOutput(Vector3 position)
        {
            State.Atoms.Add(new OutputAtom { Position = position });

            return this;
        }
        public CustomBuilder WithOutput(Vector3 position, Quaternion rotation)
        {
            State.Atoms.Add(new OutputAtom { Position = position, Rotation = rotation });

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

        public CubeBuilder WithSide(CubeSide side, SideType what)
        {
            Atom.Map.SetSide(side, what, 0, 0);

            return this;
        }

        public CubeBuilder WithSide(CubeSide side, SideType what, float offsetX, float offsetY)
        {
            Atom.Map.SetSide(side, what, offsetX, offsetY);

            return this;
        }

        public CubeBuilder WithColor(Color color)
        {
            State.Atoms.Add(new CubeColorAtom { Color = color });

            return this;
        }
    }
}
