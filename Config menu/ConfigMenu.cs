﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PiTung_Bootstrap.Config_menu
{
    public class ConfigMenu
    {
        private static ConfigMenu _Instance = new ConfigMenu();
        public static ConfigMenu Instance => _Instance;

        private static Texture2D BackTexture;

        public IList<MenuEntry> Entries = new List<MenuEntry>();
        public Vector2 Position = new Vector2(5, 50);
        public Vector2 Size = new Vector2(200, 200);

        private GUIStyle DefaultStyle;
        private Dictionary<Color, Texture2D> ColorTextures = new Dictionary<Color, Texture2D>();

        private MenuEntry SelectedEntry = null;
        private MenuEntry[] CurrentEntries = null;
        private int HoverIndex = 0;

        private ConfigMenu()
        {
            BackTexture = RoundedRectangle((int)Size.x, (int)Size.y, 7, new Color(0, 0, 0, .7f));

            DefaultStyle = new GUIStyle();
            DefaultStyle.normal.textColor = new Color(.75f, .75f, .75f);
            DefaultStyle.richText = true;

            //Entries.Add(new MenuEntry { Text = "hola" });
            //Entries.Add(new CheckboxMenuEntry { Text = "que" });
            //Entries.Add(new SimpleNumberEntry(1, 0, 10, 5) { Text = "pasa" });

            KeyCode[] keys = new[]
            {
                KeyCode.UpArrow,
                KeyCode.DownArrow,
                KeyCode.LeftArrow,
                KeyCode.RightArrow,
                KeyCode.Return
            };

            foreach (var item in keys)
            {
                ModUtilities.Input.SubscribeToKey(item, KeyDown);
            }
        }

        private void KeyDown(KeyCode key)
        {
            SimpleNumberEntry num = CurrentEntries[HoverIndex] as SimpleNumberEntry;
            CheckboxMenuEntry chk = CurrentEntries[HoverIndex] as CheckboxMenuEntry;

            if (key == KeyCode.UpArrow || key == KeyCode.DownArrow)
            {
                HoverIndex += key == KeyCode.UpArrow ? -1 : 1;

                if (HoverIndex < 0)
                    HoverIndex = 0;

                if (HoverIndex == CurrentEntries.Length)
                    HoverIndex = CurrentEntries.Length - 1;
            }
            else if (key == KeyCode.Return &&chk != null)
            {
                chk.Toggle();
            }

            if (key == KeyCode.LeftArrow)
            {
                if (num != null)
                {
                    num.Decrement();
                }
                else if (chk != null)
                {
                    chk.Toggle();
                }
            }
            else if (key == KeyCode.RightArrow)
            {
                if (num != null)
                {
                    num.Increment();
                }
                else if (chk != null)
                {
                    chk.Toggle();
                }
            }
        }
        
        private Texture2D ColorTexture(Color color)
        {
            if (!ColorTextures.ContainsKey(color))
            {
                Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                tex.SetPixels(new[] { color });
                tex.Apply();

                ColorTextures[color] = tex;
            }

            return ColorTextures[color];
        }

        public void Render()
        {
            if (CurrentEntries == null)
            {
                if (SelectedEntry == null)
                {
                    CurrentEntries = Entries.ToArray();
                    HoverIndex = 0;
                }
                else
                {
                    CurrentEntries = SelectedEntry.Children.ToArray();
                }
            }
            
            var areaStyle = new GUIStyle(DefaultStyle);
            var entryStyle = new GUIStyle(DefaultStyle)
            {
                margin = new RectOffset(5, 0, 0, 0)
            };

            float width = 200, height = 200;
            GUILayout.BeginArea(new Rect(Position, new Vector2(width, height)), BackTexture);
            
            GUILayout.Label("<size=15>PiTung Configuration</size>", new GUIStyle(DefaultStyle) { alignment = TextAnchor.MiddleCenter });

            int i = 0;
            foreach (var item in CurrentEntries)
            {
                bool drawLabel = true;
                bool hover = HoverIndex == i;

                if (item is CheckboxMenuEntry chk)
                {
                    DrawKeyValue((hover ? "> " : "") + item.Text, chk.Value ? "Yes" : "No", width, hover, entryStyle);
                }
                else if (item is SimpleNumberEntry num)
                {
                    drawLabel = false;
                    
                    string valueString = num.Value.ToString("0.0");

                    if (hover)
                    {
                        if (num.Value > num.Minimum)
                            valueString = "< " + valueString;
                        if (num.Value < num.Maximum)
                            valueString = valueString + " >";
                    }

                    DrawKeyValue((hover ? "> " : "") + num.Text, valueString, width, hover, entryStyle);
                }
                else
                {
                    string str = item.Text;

                    if (hover)
                        str = $"> <b>{str}</b>";

                    if (drawLabel)
                        GUILayout.Label(str, entryStyle);
                }

                i++;
            }

            GUILayout.EndArea();
        }

        private void DrawKeyValue(string key, string value, float width, bool hover, GUIStyle style)
        {
            GUILayout.BeginHorizontal();
            
            float totalWidth = width - 10;
            float valueWidth = style.CalcSize(new GUIContent(value)).x;

            GUILayout.Label(key, new GUIStyle(style)
            {
                fixedWidth = totalWidth - valueWidth - 5,
                fontStyle = hover ? FontStyle.Bold : FontStyle.Normal
            });
            GUILayout.Label(value, new GUIStyle(style)
            {
                fixedWidth = valueWidth,
                fontStyle = hover ? FontStyle.Bold : FontStyle.Normal
            });

            GUILayout.EndHorizontal();
        }

        private Texture2D RoundedRectangle(int w, int h, int r, Color color)
        {
            Texture2D ret = new Texture2D(w, h, TextureFormat.ARGB32, false);
            Color transparent = new Color(0, 0, 0, 0);

            ret.SetPixels(Enumerable.Repeat(transparent, w * h).ToArray());

            //Corner circles
            DrawCircle(r, r);
            DrawCircle(w - r, r);
            DrawCircle(r, h - r);
            DrawCircle(w - r, h - r);

            //Inner rectangle
            DrawRectangle(r, r, w - r, h - r);

            //Side rectangles
            DrawRectangle(r, 0, w - r, r);
            DrawRectangle(w - r, r, w, h - r);
            DrawRectangle(0, r, r, h - r);
            DrawRectangle(r, h - r, w - r, h);

            ret.Apply();
            return ret;

            void DrawRectangle(int x1, int y1, int x2, int y2)
            {
                for (int cx = x1; cx < x2; cx++)
                    for (int cy = y1; cy < y2; cy++)
                        ret.SetPixel(cx, cy, color);
            }

            void DrawCircle(int x, int y)
            {
                for (int cx = 0; cx < w; cx++)
                    for (int cy = 0; cy < h; cy++)
                        if (Mathf.Sqrt(Mathf.Pow(cx - x, 2) + Mathf.Pow(cy - y, 2)) <= r)
                            ret.SetPixel(cx, cy, color);
            }
        }
    }
}