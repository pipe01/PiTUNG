using UnityEngine;

namespace PiTung_Bootstrap
{
    public class GraphicUtilities
    {
        internal GraphicUtilities() { }

        private static readonly ObjImporter _ObjImporter = new ObjImporter();

        /// <summary>
        /// Draws <paramref name="str"/> on screen at <paramref name="position"/> with color <paramref name="color"/>
        /// </summary>
        /// <param name="str">The string to draw.</param>
        /// <param name="position">The position to draw the string at.</param>
        /// <param name="color">The string's color.</param>
        public void DrawText(string str, Vector2 position, Color color)
        {
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
    }
}
