using Harmony;
using PiTung_Bootstrap.Config_menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    internal interface IUiElement
    {
        void Draw();
    }
    internal struct UiLabel : IUiElement
    {
        private static Dictionary<Color, GUIStyle> ColorStyles = new Dictionary<Color, GUIStyle>();

        public string Text { get; private set; }
        public Vector2 Position { get; private set; }
        public Color Color { get; private set; }

        public UiLabel(string text, Vector2 position, Color color)
        {
            this.Text = text;
            this.Position = position;
            this.Color = color;
        }

        private GUIStyle GetColorStyle(Color color)
        {
            if (!ColorStyles.ContainsKey(color))
            {
                ColorStyles[color] = new GUIStyle
                {
                    normal = new GUIStyleState { textColor = color }
                };
            }

            return ColorStyles[color];
        }

        public void Draw()
        {
            var style = GetColorStyle(this.Color);
            var size = style.CalcSize(new GUIContent(this.Text));

            GUI.Label(new Rect(this.Position, size), this.Text, style);
        }
    }
}
