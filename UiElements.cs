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
        private static readonly Dictionary<Color, GUIStyle> ColorStyles = new Dictionary<Color, GUIStyle>();

        public string Text { get; }
        public Vector2 Position { get; }
        public Color Color { get; }

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
        private static readonly IDictionary<Color, GUIStyle> ColorStyles = new Dictionary<Color, GUIStyle>();

        public Rect Area { get; }

        private readonly GUIStyle Style;

        public UiRect(Rect area, Color color)
        {
            this.Area = area;

            if (!ColorStyles.ContainsKey(color))
            {
                Texture2D bg = new Texture2D(1, 1);
                bg.SetPixel(0, 0, color);
                bg.Apply();

                ColorStyles[color] = new GUIStyle()
                {
                    normal = new GUIStyleState { background = bg }
                };
            }

            this.Style = ColorStyles[color];
        }

        public void Draw()
        {
            GUI.Box(this.Area, "", this.Style);
        }
    }
}
