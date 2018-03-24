using PiTung.Mod_utilities;
using System;
using PiTung.Building;
using PiTung.Config_menu;
using Harmony;
using PiTung.Console;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
using References;

#pragma warning disable RCS1102 // Make class static.
#pragma warning disable RCS1213 // Remove unused member declaration.
#pragma warning disable RCS1018 // Add default access modifier.
#pragma warning disable RCS1037 // Add default access modifier.

namespace PiTung
{
    [HarmonyPatch(typeof(DummyComponent), "Update")]
    internal class DummyUpdatePatch
    {
        static void Prefix()
        {
            IGConsole.Update();
            HologramManager.Update();

            Mod.CallOnAllMods(o => o.Update());
        }
    }

    //[HarmonyPatch(typeof(LoadGame), "Load")]
    //internal class LoadGameLoadPatch
    //{
    //    static void Postfix()
    //    {
    //        BoardManager.Instance.Reset();

    //        Mod.CallOnAllMods(o => o.OnWorldLoaded(SaveManager.SaveName));
    //    }
    //}

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
            //TODO: Maybe bring this back if I ever properly implement the config menu.
            //if (Input.GetKeyDown(KeyCode.F5))
            //{
            //    ConfigMenu.Instance.Show = !ConfigMenu.Instance.Show;

            //    UIManager.SomeOtherMenuIsOpen = ConfigMenu.Instance.Show;

            //    if (ConfigMenu.Instance.Show)
            //    {
            //        UIManager.UnlockMouseAndDisableFirstPersonLooking();
            //    }
            //    else
            //    {
            //        UIManager.LockMouseAndEnableFirstPersonLooking();
            //    }
            //}

            foreach (var key in KeyCodesToListenTo)
            {
                if ((Input.GetKeyDown(key.Key))
                    || (key.Repeat && Input.GetKey(key.Key) && Time.time - PressedTimes[key] >= key.RepeatInterval))
                {
                    PressedTimes[key] = Time.time;
                    KeyDown?.Invoke(key.Key);
                }
            }

            ModInput.UpdateListeners();
        }
    }

    [HarmonyPatch(typeof(DummyComponent), nameof(DummyComponent.OnGUI))]
    internal class GuiPatch
    {
        public static List<IUiElement> ElementsToBeDrawn = new List<IUiElement>();

        static void Prefix()
        {
            if (ModUtilities.IsOnMainMenu && !IGConsole.Shown)
            {
                var ver = new Version(PiTUNG.FrameworkVersion.Major, PiTUNG.FrameworkVersion.Minor, PiTUNG.FrameworkVersion.Build);
                string str = $"<b>PiTUNG v{ver} enabled!</b>\nLoaded mods: " + Bootstrapper.ModCount;

                if (UpdateChecker.IsUpdateAvailable)
                {
                    ModUtilities.Graphics.DrawText("<b>Update available</b>", new Vector2(6, 36), Color.black);
                    ModUtilities.Graphics.DrawText("<b><color=#00ff00>Update available</color></b>", new Vector2(5, 35), Color.white);
                }

                ModUtilities.Graphics.DrawText(str, new Vector2(5, 5), Color.white, true);
            }

            HologramManager.Draw();

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

    [HarmonyPatch(typeof(DummyComponent), nameof(DummyComponent.OnApplicationQuit))]
    internal class QuitPatch
    {
        static void Prefix()
        {
            Mod.CallOnAllMods(o => o.OnApplicationQuit());
        }
    }

    [HarmonyPatch(typeof(DummyComponent), nameof(DummyComponent.Awake))]
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
    
    [HarmonyPatch(typeof(CircuitBoard), "Awake")]
    internal class CircuitBoardAwakePatch
    {
        static void Postfix(CircuitBoard __instance)
        {
            BoardManager.Instance.OnBoardAdded(__instance.x, __instance.z, __instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(SelectionMenu), "Awake")]
    internal class BuildMenuPatch
    {
        static void Postfix(SelectionMenu __instance)
        {
            var objs = __instance.PlaceableObjectTypes.Select(Prefabs.ComponentTypeToPrefab);

            Components.AddComponents(objs.ToList());
        }
    }

    //[HarmonyPatch(typeof(StuffDeleter), nameof(StuffDeleter.DestroyIIConnection))]
    //internal class StuffDeleterIIPatch
    //{
    //    static void Prefix(InputInputConnection connection)
    //    {
    //        var kvp = new KeyValuePair<CircuitInput, CircuitInput>(connection.Point1, connection.Point2);

    //        if (Builder.PendingIIConnections.Contains(kvp))
    //            Builder.PendingIIConnections.Remove(kvp);
    //    }
    //}

    //[HarmonyPatch(typeof(StuffDeleter), nameof(StuffDeleter.DestroyIOConnection))]
    //internal class StuffDeleterIOPatch
    //{
    //    static void Prefix(InputOutputConnection connection)
    //    {
    //        var kvp = new KeyValuePair<CircuitInput, Output>(connection.Point1, connection.Point2);

    //        if (Builder.PendingIOConnections.Contains(kvp))
    //            Builder.PendingIOConnections.Remove(kvp);
    //    }
    //}

    [HarmonyPatch(typeof(StuffDeleter), nameof(StuffDeleter.DeleteThing))]
    internal class StuffDeleterBoardPatch
    {
        static void Prefix(GameObject DestroyThis)
        {
            BoardManager.Instance.OnBoardDeleted(DestroyThis);
        }
    }

    [HarmonyPatch(typeof(BoardPlacer), nameof(BoardPlacer.CancelPlacement))]
    internal class BoardPlacerCancelPatch
    {
        static void Prefix()
        {
            BoardManager.Instance.OnBoardDeleted(BoardPlacer.BoardBeingPlaced);
        }
    }
}
