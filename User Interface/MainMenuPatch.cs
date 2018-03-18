using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PiTung.User_Interface
{
    [HarmonyPatch(typeof(RunMainMenu), "Start")]
    internal static class RunMainMenuPatch
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
