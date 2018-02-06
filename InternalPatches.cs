using PiTung_Bootstrap.Building;
using PiTung_Bootstrap.Config_menu;
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

            Mod.CallOnAllMods(o => o.Update());
        }
    }
    
    [HarmonyPatch(typeof(LoadGame), "Load")]
    internal class LoadGameLoadPatch
    {
        static void Postfix()
        {
            Mod.CallOnAllMods(o => o.OnWorldLoaded(SaveManager.SaveName));
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
            if (Input.GetKeyDown(KeyCode.F5))
            {
                ConfigMenu.Instance.Show = !ConfigMenu.Instance.Show;

                UIManager.SomeOtherMenuIsOpen = ConfigMenu.Instance.Show;

                if (ConfigMenu.Instance.Show)
                {
                    UIManager.UnlockMouseAndDisableFirstPersonLooking();
                }
                else
                {
                    UIManager.LockMouseAndEnableFirstPersonLooking();
                }
            }

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
            if (ModUtilities.IsOnMainMenu && !IGConsole.Shown)
            {
                string str = $"<b>PiTUNG v{PiTung.FrameworkVersion} enabled!</b>\nLoaded mods: " + Bootstrapper.ModCount;

                if (UpdateChecker.IsUpdateAvailable)
                {
                    ModUtilities.Graphics.DrawText("<b>Update available</b>", new Vector2(6, 36), Color.black);
                    ModUtilities.Graphics.DrawText("<b><color=#00ff00>Update available</color></b>", new Vector2(5, 35), Color.white);
                }

                ModUtilities.Graphics.DrawText(str, new Vector2(5, 5), Color.white, true);
            }

            foreach (var item in ElementsToBeDrawn)
            {
                item.Draw();
            }
            ElementsToBeDrawn.Clear();

            Mod.CallOnAllMods(o => o.OnGUI());

            ConfigMenu.Instance.Render();
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

    [HarmonyPatch(typeof(BoardPlacer), "PlaceBoard")]
    internal class BoardPlacerPatch
    {
        static void Prefix()
        {
            var obj = ModUtilities.GetStaticFieldValue<BoardPlacer, GameObject>("BoardBeingPlaced");
            var component = obj.GetComponent<CircuitBoard>();

            BoardManager.Instance.BoardAdded(component.x, component.z, obj);
        }
    }

    [HarmonyPatch(typeof(BuildMenu), "Awake")]
    internal class BuildMenuPatch
    {
        static void Postfix(BuildMenu __instance)
        {
            foreach (var item in __instance.PlaceableObjects)
            {
                MDebug.WriteLine(item.name);
            }
        }
    }
}
