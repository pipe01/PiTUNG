using PiTung.Mod_utilities;
using System;
using PiTung.Config_menu;
using Harmony;
using PiTung.Console;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using PiTung.User_Interface;

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
            Mod.CallOnAllMods(o => o.Update());
        }
    }

    [HarmonyPatch(typeof(DummyComponent), "LateUpdate")]
    internal class DummyLateUpdatePatch
    {
        static void Prefix()
        {
            HologramManager.Update();
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.LoadAll))]
    internal class LoadGameLoadPatch
    {
        static void Postfix()
        {
            Mod.CallOnAllMods(o => o.OnWorldLoaded(SaveManager.SaveName));

            SelectionMenu.Instance.PlaceableObjectTypes.Add(ComponentType.CustomObject);
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

                ModsScreen.Instance.Draw();
            }

            HologramManager.Draw();

            foreach (var item in ElementsToBeDrawn)
            {
                item.Draw();
            }
            ElementsToBeDrawn.Clear();

            Mod.CallOnAllMods(o => o.OnGUI());
            
            Components.CustomMenu.Instance.Draw();
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
}
