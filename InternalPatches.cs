using Harmony;
using System.Collections.Generic;
using UnityEngine;

namespace PiTung_Bootstrap
{
    [HarmonyPatch(typeof(DummyComponent), "Update")]
    internal class DummyUpdatePatch
    {
        static void Prefix()
        {
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
        public static List<KeyCode> KeyCodesToListenTo = new List<KeyCode>();

        public delegate void KeyDownDelegate(KeyCode keyCode);
        public static event KeyDownDelegate KeyDown;

        static void Prefix()
        {
            foreach (var key in KeyCodesToListenTo)
            {
                if (Input.GetKeyDown(key))
                {
                    KeyDown?.Invoke(key);
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
        }
    }

    [HarmonyPatch(typeof(DummyComponent), "Awake")]
    internal class AwakePatch
    {
        static void Prefix(DummyComponent __instance)
        {
            if (UnityEngine.Object.FindObjectsOfType(__instance.GetType()).Length > 1)
            {
                UnityEngine.Object.Destroy(__instance);
            }
        }
    }
}
