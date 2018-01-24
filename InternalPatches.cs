using System.Windows.Input;
using Harmony;
using PiTung_Bootstrap.Console;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable RCS1102 // Make class static.
#pragma warning disable RCS1213 // Remove unused member declaration.
#pragma warning disable RCS1018 // Add default access modifier.
#pragma warning disable RCS1037 // Add default access modifier.

namespace PiTung_Bootstrap
{
    [HarmonyPatch(typeof(DummyComponent), "Update")]
    internal class DummyUpdatePatch
    {
        static void Prefix()
        {
            IGConsole.Update();
            foreach (var item in Mod.AliveMods)
            {
                item.Update();
            }
        }
    }
    
    [HarmonyPatch(typeof(LoadGame), "Load")]
    internal class LoadGameLoadPatch
    {
        static void Postfix()
        {
            foreach (var item in Mod.AliveMods)
            {
                item.LodingWorld(SaveManager.SaveName);
            }
        }
    }

    [HarmonyPatch(typeof(DummyComponent), "Update")]
    internal class InputPatch
    {
        public struct KeyStruct
        {
            public KeyCode Key { get; }
            public bool Repeat { get; }
            public float RepeatInterval { get; }

            public KeyStruct(KeyCode key, bool repeat, float repeatInterval)
            {
                this.Key = key;
                this.Repeat = repeat;
                this.RepeatInterval = repeatInterval;
            }
        }

        public static List<KeyStruct> KeyCodesToListenTo = new List<KeyStruct>();

        private static Dictionary<KeyStruct, float> PressedTimes = new Dictionary<KeyStruct, float>();

        public delegate void KeyDownDelegate(KeyCode keyCode);
        public static event KeyDownDelegate KeyDown;
        
        static void Prefix()
        {
            foreach (var key in KeyCodesToListenTo)
            {
                if ((Input.GetKeyDown(key.Key))
                    || (key.Repeat && Input.GetKey(key.Key) && Time.time - PressedTimes[key] >= key.RepeatInterval))
                {
                    PressedTimes[key] = Time.time;
                    KeyDown?.Invoke(key.Key);
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(DummyComponent), "OnGUI")]
    internal class GuiPatch
    {
        public static List<IUiElement> ElementsToBeDrawn = new List<IUiElement>();

        static void Prefix()
        {
            foreach (var item in ElementsToBeDrawn)
            {
                item.Draw();
            }
            ElementsToBeDrawn.Clear();
            
            foreach (var item in Mod.AliveMods)
            {
                item.OnGUI();
            }

            Config_menu.ConfigMenu.Instance.Render();
            IGConsole.Draw(); // Drawn last so that it stays on top
        }
    }

    [HarmonyPatch(typeof(DummyComponent), "Awake")]
    internal class AwakePatch
    {
        static void Prefix(DummyComponent __instance)
        {
            if (Object.FindObjectsOfType(__instance.GetType()).Length > 1)
            {
                Object.Destroy(__instance);
            }
        }
    }
}
