using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung_Bootstrap
{
    public class GraphicUtilities
    {
        internal GraphicUtilities() { }

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

        public void DrawRect(Rect area, Color color)
        {
            GuiPatch.ElementsToBeDrawn.Add(new UiRect(area, color));
        }
    }
}
