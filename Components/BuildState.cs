using System;
using System.Collections.Generic;
using UnityEngine;

namespace PiTung.Components
{
    internal class BuildState
    {
        internal IList<IAtom> Atoms = new List<IAtom>();
        internal IList<Type> Components = new List<Type>();
        internal IStructureAtom Structure;

        internal GameObject BuildResult()
        {
            GameObject root = Structure.GetRootObject();

            foreach (var item in Atoms)
            {
                item.AddToGameObject(root);
            }

            foreach (var item in Components)
            {
                root.AddComponent(item);
            }

            return root;
        }
    }
}
