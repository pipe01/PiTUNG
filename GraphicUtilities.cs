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

        public void DrawText(string str, Vector2 position, Color color)
        {
            GuiPatch.ElementsToBeDrawn.Add(new UiLabel(str, position, color));
        }
    }
}
