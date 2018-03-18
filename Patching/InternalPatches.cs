using UnityEngine.UI;
using System.Reflection;
using PiTung.Mod_utilities;
using System;
using PiTung.Building;
using PiTung.Config_menu;
using Harmony;
using PiTung.Console;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

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
    
    [HarmonyPatch(typeof(LoadGame), "Load")]
    internal class LoadGameLoadPatch
    {
        static void Postfix()
        {
            BoardManager.Instance.Reset();
            
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

    [HarmonyPatch(typeof(BuildMenu), "Awake")]
    internal class BuildMenuPatch
    {
        static void Postfix(BuildMenu __instance)
        {
            Components.AddComponents(__instance.PlaceableObjects);
        }
    }

    [HarmonyPatch(typeof(StuffDeleter), nameof(StuffDeleter.DestroyIIConnection))]
    internal class StuffDeleterIIPatch
    {
        static void Prefix(InputInputConnection connection)
        {
            var kvp = new KeyValuePair<CircuitInput, CircuitInput>(connection.Point1, connection.Point2);

            if (Builder.PendingIIConnections.Contains(kvp))
                Builder.PendingIIConnections.Remove(kvp);
        }
    }

    [HarmonyPatch(typeof(StuffDeleter), nameof(StuffDeleter.DestroyIOConnection))]
    internal class StuffDeleterIOPatch
    {
        static void Prefix(InputOutputConnection connection)
        {
            var kvp = new KeyValuePair<CircuitInput, Output>(connection.Point1, connection.Point2);

            if (Builder.PendingIOConnections.Contains(kvp))
                Builder.PendingIOConnections.Remove(kvp);
        }
    }
    
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

    [HarmonyPatch(typeof(RunMainMenu), "Start")]
    internal class RunMainMenuPatch
    {
        static Canvas MainMenuCanvas, PitungCanvas;

        static void Prefix(RunMainMenu __instance)
        {
            //Get the main menu canvas
            RunMainMenu.Instance = __instance;
            MainMenuCanvas = __instance.MainMenuCanvas;

            //Get the "New Game" button
            var originalButton = MainMenuCanvas.transform.GetChild(1).gameObject;

            //Create a clone of the previous button
            var button = GameObject.Instantiate(originalButton);

            //Tint button color
            button.GetComponent<Image>().color = Color.yellow;
           
            //Set its parent to the canvas
            button.transform.SetParent(MainMenuCanvas.transform, false);

            //Move the button to its position
            var rect = button.GetComponent<RectTransform>();
            rect.anchoredPosition = GetPitungButtonAnchoredPosition();
            
            //Set the new button's text
            SetLabelText(button, "PiTUNG");

            //Add a listener to the button's onClick button
            var btnComponent = button.GetComponent<UnityEngine.UI.Button>();
            btnComponent.onClick.AddListener(PitungButtonClicked);

            CreatePitungCanvas();
        }
        
        private static Vector2 GetPitungButtonAnchoredPosition()
        {
            //Get the last button and the one before it
            var optionsButton = MainMenuCanvas.transform.GetChild(3);
            var aboutButton = MainMenuCanvas.transform.GetChild(4);

            //Get their RectTransform's
            var optionsRect = optionsButton.GetComponent<RectTransform>();
            var aboutRect = aboutButton.GetComponent<RectTransform>();

            //Calculate the distance between the two buttons
            float yDifference = aboutRect.anchoredPosition.y - optionsRect.anchoredPosition.y;

            //Return a vector thats `yDifference` pixels below the last button
            return new Vector2(aboutRect.anchoredPosition.x, aboutRect.anchoredPosition.y + yDifference);
        }

        private static void CreatePitungCanvas()
        {
            //Create a new canvas, clear it and set its name
            PitungCanvas = GameObject.Instantiate(MainMenuCanvas, null);
            PitungCanvas.transform.DetachChildren();
            PitungCanvas.enabled = false;
            PitungCanvas.name = "PiTUNG Canvas";

            //Get the new game canvas' back button and clone it
            var original = RunMainMenu.Instance.NewGameCanvas.transform.GetChild(2).gameObject;
            var backButton = GameObject.Instantiate(original, PitungCanvas.transform);
            
            //Make it bigger because for some reason it gets smaller when cloned, as if it went through a Unity washing machine
            var backRect = backButton.GetComponent<RectTransform>();
            backRect.localScale *= 1.2f;

            //Set its click listener
            var backBtnComponent = backButton.GetComponent<UnityEngine.UI.Button>();
            backBtnComponent.onClick.AddListener(BackButtonClicked);

            //Set its text
            SetLabelText(backButton, "Back");
        }
        
        private static void BackButtonClicked()
        {
            PitungCanvas.enabled = false;
            MainMenuCanvas.enabled = true;
            RunMainMenu.Instance.NewGameCanvas.enabled = false;
        }

        private static void PitungButtonClicked()
        {
            MainMenuCanvas.enabled = false;
            PitungCanvas.enabled = true;
            RunMainMenu.Instance.NewGameCanvas.enabled = false;
        }
        
        private static void SetLabelText(GameObject obj, string text)
        {
            foreach (var item in obj.GetComponentsInChildren<Component>())
            {
                var type = item.GetType();

                //Hacky way of setting the text to avoid importing the DLL
                if (type.Name == "TextMeshProUGUI")
                {
                    type.InvokeMember("SetText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, item, new object[] { text });
                }
            }
        }
    }
}
