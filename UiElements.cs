using System.Collections.Generic;
using UnityEngine;

namespace PiTung_Bootstrap
{
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

    internal struct UiRect : IUiElement
    {
        public Rect Area { get; private set; }

        private GUIStyle style;

        public UiRect(Rect area, Color color)
        {
            this.Area = area;

            Texture2D bg = new Texture2D(1, 1);
            bg.SetPixel(0, 0, color);
            bg.Apply();

            this.style = new GUIStyle()
            {
                normal = new GUIStyleState { background = bg }
            };
        }

        public void Draw()
        {
            GUI.Box(this.Area, "", this.style);
        }
    }
}
