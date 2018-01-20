using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung_Bootstrap
{
    public class InputUtilities
    {
        internal InputUtilities() { }
        
        internal delegate void KeyDownDelegate(KeyCode keyCode);

        internal void SubscribeToKey(KeyCode keyCode, KeyDownDelegate keyDownEvent)
        {
            InputPatch.KeyDown += o =>
            {
                if (o == keyCode) keyDownEvent(keyCode);
            };

            if (!InputPatch.KeyCodesToListenTo.Contains(keyCode))
                InputPatch.KeyCodesToListenTo.Add(keyCode);
        }
    }
}
