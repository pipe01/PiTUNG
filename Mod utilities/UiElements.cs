using System.Collections.Generic;
using UnityEngine;

namespace PiTung
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
        /// <summary>
        /// This will store textures that correspond to a specific color.
        /// </summary>
        private static readonly IDictionary<Color, Texture2D> TextureCache = new Dictionary<Color, Texture2D>();

        public Rect Area { get; }
        
        private Texture2D Texture { get; }

        public UiRect(Rect area, Color color)
        {
            this.Area = area;
            
            if (!TextureCache.ContainsKey(color))
            {
                var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                tex.SetPixel(0, 0, color);
                tex.Apply();

                TextureCache[color] = tex;
            }

            this.Texture = TextureCache[color];
        }
        
        public void Draw()
        {
            GUI.DrawTexture(this.Area, this.Texture);
        }
    }
}
