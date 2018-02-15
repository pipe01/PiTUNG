using UnityEngine;

namespace PiTung
{
    internal class InputUtilities
    {
        private InputUtilities() { }

        public static InputUtilities Instance { get; } = new InputUtilities();

        public delegate void KeyDownDelegate(KeyCode keyCode);

        /// <summary>
        /// Fires <paramref name="keyDownEvent"/> when <paramref name="keyCode"/> is down.
        /// </summary>
        /// <param name="keyCode">The key.</param>
        /// <param name="keyDownEvent">The method to execute.</param>
        /// <param name="repeat">If true, the key event will be continuously while the key is down.</param>
        /// <param name="repeatInterval">The interval between key presses if <paramref name="repeat"/> is true.</param>
        public void SubscribeToKey(KeyCode keyCode, KeyDownDelegate keyDownEvent, bool repeat = false, 
            float repeatInterval = 0.1f)
        {
            InputPatch.KeyDown += o =>
            {
                if (o == keyCode) keyDownEvent(keyCode);
            };

            var key = new InputPatch.KeyStruct(keyCode, repeat, repeatInterval);

            if (!InputPatch.KeyCodesToListenTo.Contains(key))
                InputPatch.KeyCodesToListenTo.Add(key);
        }
    }
}
