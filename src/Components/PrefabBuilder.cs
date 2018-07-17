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

        public CustomBuilder WithInput(float x, float y, float z) => WithInput(x, y, z, null);
        public CustomBuilder WithInput(float x, float y, float z, string description)
        {
            return WithInput(new Vector3(x, y, z), description);
        }

        public CustomBuilder WithInput(Vector3 position) => WithInput(position, null);
        public CustomBuilder WithInput(Vector3 position, string description)
        {
            State.Atoms.Add(new InputPegAtom { Position = position, Description = description });

            return this;
        }

        public CustomBuilder WithInput(Vector3 position, Quaternion rotation) => WithInput(position, rotation, null);
        public CustomBuilder WithInput(Vector3 position, Quaternion rotation, string description)
        {
            State.Atoms.Add(new InputPegAtom { Position = position, Rotation = rotation, Description = description });

            return this;
        }

        public CustomBuilder WithOutput(float x, float y, float z) => WithOutput(x, y, z, null);
        public CustomBuilder WithOutput(float x, float y, float z, string description)
        {
            return WithOutput(new Vector3(x, y, z), description);
        }

        public CustomBuilder WithOutput(Vector3 position) => WithOutput(position, null);
        public CustomBuilder WithOutput(Vector3 position, string description)
        {
            State.Atoms.Add(new OutputAtom { Position = position, Description = description });

            return this;
        }

        public CustomBuilder WithOutput(Vector3 position, Quaternion rotation) => WithOutput(position, rotation, null);
        public CustomBuilder WithOutput(Vector3 position, Quaternion rotation, string description)
        {
            State.Atoms.Add(new OutputAtom { Position = position, Rotation = rotation, Description = description });

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

        [Obsolete("Use WithIO instead.")]
        public CubeBuilder WithSide(CubeSide side, SideType what) => WithIO(side, what);

        public CubeBuilder WithIO(CubeSide side, SideType what)
            => WithIO(side, what, null);

        public CubeBuilder WithIO(CubeSide side, SideType what, string description)
        {
            Atom.Map.SetSide(side, what, 0, 0, description);

            return this;
        }

        public CubeBuilder WithIO(CubeSide side, SideType what, float offsetX, float offsetY)
            => WithIO(side, what, offsetX, offsetY, null);

        public CubeBuilder WithIO(CubeSide side, SideType what, float offsetX, float offsetY, string description)
        {
            Atom.Map.SetSide(side, what, offsetX, offsetY, description);

            return this;
        }

        public CubeBuilder WithColor(Color color)
        {
            State.Atoms.Add(new CubeColorAtom { Color = color });

            return this;
        }
    }
}
