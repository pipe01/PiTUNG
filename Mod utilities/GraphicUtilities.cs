using System.Collections.Generic;
using UnityEngine;

namespace PiTung
{
    /// <summary>
    /// Contains various graphical utilities.
    /// </summary>
    public class GraphicUtilities
    {
        internal GraphicUtilities() { }

        private static readonly ObjImporter _ObjImporter = new ObjImporter();

        private int SphereCounter = 0;
        private Dictionary<int, GameObject> Spheres = new Dictionary<int, GameObject>();
        private Stack<GameObject> SphereCache = new Stack<GameObject>();

        /// <summary>
        /// Draws <paramref name="str"/> on screen at <paramref name="position"/> with color <paramref name="color"/>
        /// </summary>
        /// <param name="str">The string to draw.</param>
        /// <param name="position">The position to draw the string at.</param>
        /// <param name="color">The string's color.</param>
        public void DrawText(string str, Vector2 position, Color color, bool shadow = false, int shadowOffset = 1)
        {
            if (shadow)
            {
                GuiPatch.ElementsToBeDrawn.Add(new UiLabel(str, new Vector2(position.x + shadowOffset, position.y + shadowOffset), Color.black));
            }

            GuiPatch.ElementsToBeDrawn.Add(new UiLabel(str, position, color));
        }

        /// <summary>
        /// Fills an <paramref name="area"/> on screen with color.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="color"></param>
        public void DrawRect(Rect area, Color color)
        {
            GuiPatch.ElementsToBeDrawn.Add(new UiRect(area, color));
        }

        /// <summary>
        /// Imports a mesh from a .obj file.
        /// </summary>
        /// <param name="filePath">The obj file's absolute path.</param>
        /// <returns>The obj file transformed into a mesh.</returns>
        public Mesh ImportMeshFromFile(string filePath)
        {
            return _ObjImporter.ImportFile(filePath);
        }

        /// <summary>
        /// Creates a sphere at <paramref name="pos"/> with size <paramref name="size"/> and color <paramref name="color"/>.
        /// </summary>
        /// <param name="pos">The sphere's center.</param>
        /// <param name="size">The sphere's size.</param>
        /// <param name="color">The sphere's color.</param>
        /// <returns>The sphere's ID. See <see cref="DestroySphere(int)"/>.</returns>
        public int CreateSphere(Vector3 pos, float size = 1, Color? color = null)
        {
            GameObject obj;

            if (SphereCache.Count == 0)
            {
                obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(obj.GetComponent<Collider>());
            }
            else
            {
                obj = SphereCache.Pop();
            }

            obj.transform.position = pos;
            obj.transform.localScale = Vector3.one * (size * 0.1f);
            obj.GetComponent<Renderer>().material.color = color ?? Color.green;
            
            int id = SphereCounter++;

            Spheres.Add(id, obj);
            return id;
        }

        /// <summary>
        /// Destroys a sphere with ID <paramref name="id"/> previously spawned by <see cref="CreateSphere(Vector3, float, Color?)"/>.
        /// </summary>
        /// <param name="id">The ID of the sphere we want to destroy.</param>
        public void DestroySphere(int id)
        {
            if (Spheres.TryGetValue(id, out var s))
            {
                s.SetActive(false);

                SphereCache.Push(s);
                Spheres.Remove(id);
            }
        }
    }
}
