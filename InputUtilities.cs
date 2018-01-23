using UnityEngine;

namespace PiTung_Bootstrap
{
    public class InputUtilities
    {
        internal InputUtilities() { }

        internal delegate void KeyDownDelegate(KeyCode keyCode);

        /// <summary>
        /// Fires <paramref name="keyDownEvent"/> when <paramref name="keyCode"/> is down.
        /// </summary>
        /// <param name="keyCode">The key.</param>
        /// <param name="keyDownEvent">The method to execute.</param>
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
