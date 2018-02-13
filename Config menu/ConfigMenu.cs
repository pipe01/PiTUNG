using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PiTung.Config_menu
{
    internal class ConfigMenu
    {
        public static ConfigMenu Instance { get; } = new ConfigMenu();

        public bool Show { get; set; }

        private static Texture2D BackTexture;

        public IList<MenuEntry> Entries = new List<MenuEntry>();
        public Vector2 Position = new Vector2(5, 50);
        public Vector2 Size = new Vector2(200, 200);

        private readonly GUIStyle DefaultStyle;

        private int VisibleEntries => (int)Math.Floor(Size.y / DefaultStyle.CalcSize(new GUIContent("Test")).y) - 1;

        private MenuEntry CurrentParent = null;
        private int HoverIndex = 0;
        private int ItemsOffset = 0;
        private Stack<KeyValuePair<int, int>> HoverStack = new Stack<KeyValuePair<int, int>>();

        private MenuEntry[] CurrentEntries
        {
            get
            {
                var original = CurrentParent?.Children ?? Entries;
                var copy = new List<MenuEntry>(original);
                
                if (CurrentParent != null)
                    copy.Insert(0, new GoUpMenuEntry());

                return copy.ToArray();
            }
        }
        
        private ConfigMenu()
        {
            BackTexture = RoundedRectangle((int)Size.x, (int)Size.y, 7, new Color(0, 0, 0, .7f));

            DefaultStyle = new GUIStyle();
            DefaultStyle.normal.textColor = new Color(.75f, .75f, .75f);
            DefaultStyle.richText = true;
            
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
                ModUtilities.Input.SubscribeToKey(item, KeyDown, true, 0.15f);
            }
        }

        private void KeyDown(KeyCode key)
        {
            if (!Show)
                return;

            MenuEntry hover = CurrentEntries[HoverIndex];
            var chk = hover as CheckboxMenuEntry;

            if (key == KeyCode.UpArrow || key == KeyCode.DownArrow)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    HoverIndex = key == KeyCode.UpArrow ? 0 : CurrentEntries.Length - 1;
                }
                else
                {
                    HoverIndex += key == KeyCode.UpArrow ? -1 : 1;
                }

                if (HoverIndex < 0)
                    HoverIndex = 0;

                if (HoverIndex == CurrentEntries.Length)
                    HoverIndex = CurrentEntries.Length - 1;
            }
            else if (key == KeyCode.Return)
            {
                if (chk != null)
                {
                    chk.Toggle();
                }
                else if (hover is GoUpMenuEntry goUp)
                {
                    this.CurrentParent = goUp.Parent;

                    var popped = HoverStack.Pop();

                    HoverIndex = popped.Key;
                    ItemsOffset = popped.Value;
                }
                else if (hover.Children?.Count > 0)
                {
                    this.CurrentParent = hover;
                    HoverStack.Push(new KeyValuePair<int, int>(HoverIndex, ItemsOffset));
                    HoverIndex = 0;
                    ItemsOffset = 0;
                }
            }

            var num = hover as SimpleNumberEntry;

            if (key == KeyCode.LeftArrow && hover is SimpleNumberEntry test)
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

            while (UpdateScroll()) ;
        }
        
        private bool UpdateScroll()
        {
            bool goDown = HoverIndex - ItemsOffset + 2 > VisibleEntries,
                 goUp = ItemsOffset > 0 && HoverIndex - ItemsOffset + 2 < VisibleEntries;

            if (goDown)
            {
                ItemsOffset++;
            }
            else if (goUp)
            {
                ItemsOffset--;
            }

            return goDown || goUp;
        }

        public void Render()
        {
            if (!Show)
                return;

            var areaStyle = new GUIStyle(DefaultStyle);
            var entryStyle = new GUIStyle(DefaultStyle)
            {
                margin = new RectOffset(5, 0, 0, 0)
            };

            float width = Size.x, height = Size.y;

            GUILayout.BeginArea(new Rect(Position, new Vector2(width, height)), BackTexture);

            GUILayout.Label("<size=15>PiTung Configuration</size>", new GUIStyle(DefaultStyle) { alignment = TextAnchor.MiddleCenter });
            
            int i = 0;
            foreach (var item in CurrentEntries.Skip(ItemsOffset).Take(VisibleEntries))
            {
                bool drawLabel = true;
                bool hover = HoverIndex - ItemsOffset == i;

                if (item is CheckboxMenuEntry chk)
                {
                    DrawKeyValue((hover ? "> " : "") + chk.Text, chk.Value ? "Yes" : "No", width, hover, entryStyle);
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
                            valueString += " >";
                    }

                    DrawKeyValue((hover ? "> " : "") + num.Text, valueString, width, hover, entryStyle);
                }
                else if (item is TextMenuEntry text)
                {
                    string str = text.Text;

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
