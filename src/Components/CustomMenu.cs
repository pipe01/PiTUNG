using PiTung.Console;
using System.Diagnostics;
using UnityEngine;

namespace PiTung.Components
{
    internal class CustomMenu
    {
        public static CustomMenu Instance { get; } = new CustomMenu();

        private const int FontSize = 15;

        private GUIStyle NormalStyle, SelectedStyle, ModHeaderStyle;
        private readonly KeyCode[] NumberKeys = new[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };
        private float LastActionTime = 0;
        private bool AutoHidden = false;

        private bool _visible;
        public bool Visible
        {
            get => _visible;
            set
            {
                AutoHidden = false;
                _visible = value;

                if (value)
                    LastActionTime = Time.time;
            }
        }

        public int Selected { get; private set; }
        public bool SelectionChanged { get; private set; }
        public bool ModCategories { get; set; }
        public float VisibleTime { get; set; } = Settings.Get("SelectionMenuOpenTime", 0.8f);

        private bool ShouldHide => Time.time - LastActionTime >= VisibleTime;

        private CustomMenu()
        {
            Initialize();
        }

        private void Initialize()
        {
            NormalStyle = new GUIStyle
            {
                normal = new GUIStyleState
                {
                    background = ModUtilities.Graphics.CreateSolidTexture(1, 1, new Color(0, 0, 0, 0.2f)),
                    textColor = Color.white
                },
                fontSize = FontSize,
                padding = new RectOffset(1, 1, 1, 1)
            };

            SelectedStyle = new GUIStyle(NormalStyle)
            {
                normal = new GUIStyleState
                {
                    background = ModUtilities.Graphics.CreateSolidTexture(1, 1, new Color(0, 0.3f, 0, 0.3f)),
                    textColor = Color.cyan
                }
            };

            ModHeaderStyle = new GUIStyle(NormalStyle)
            {
                normal = new GUIStyleState
                {
                    background = ModUtilities.Graphics.CreateSolidTexture(1, 1, new Color(1, 0.8f, 0, 0.4f))
                }
            };
        }

        private float widestX = 100;
        public void Draw()
        {
            if (ModUtilities.IsOnMainMenu || !Visible || AutoHidden)
                return;

            if (ShouldHide)
            {
                AutoHidden = true;
                return;
            }

            int i = 0;
            float currentX = 40;
            float currentY = 40;
            Mod lastMod = null;

            foreach (var item in ComponentRegistry.Registry.Values)
            {
                if (ModCategories && item.Mod != lastMod)
                {
                    lastMod = item.Mod;
                    
                    currentY += NormalStyle.CalcSize(new GUIContent("A")).y;
                    currentY += DrawEntry(item.Mod.Name, ModHeaderStyle, currentX, currentY);
                }

                GUIStyle style = i++ == Selected ? SelectedStyle : NormalStyle;

                currentY += DrawEntry(item.DisplayName, style, currentX, currentY);
                if (currentY > Screen.height - 40)
                {
                    currentY = 40;
                    currentX += widestX + 40;
                    widestX = 100;
                }
            }

            float DrawEntry(string text, GUIStyle style, float x, float y)
            {
                var size = style.CalcSize(new GUIContent(text));
                widestX = size.x > widestX ? size.x : widestX;
                if (size.x < 100)
                    size.x = 100;
                else
                    size.x += 3;

                GUI.Label(new Rect(x, y, size.x, size.y), text, style);

                return size.y;
            }
        }

        public void Update()
        {
            SelectionChanged = false;

            if (Input.GetKey(KeyCode.LeftControl))
            {
                int previous = Selected;

                if (GameplayUIManager.ScrollUp(false))
                {
                    Selected--;
                }
                else if (GameplayUIManager.ScrollDown(false))
                {
                    Selected++;
                }

                for (int i = 0; i < NumberKeys.Length; i++)
                {
                    if (Input.GetKeyDown(NumberKeys[i]))
                    {
                        Selected = i;
                        break;
                    }
                }
                
                if (Selected >= ComponentRegistry.Registry.Count)
                {
                    Selected = 0;
                }

                if (Selected < 0)
                {
                    Selected = ComponentRegistry.Registry.Count - 1;
                }

                if (Selected != previous)
                {
                    SelectionChanged = true;
                    LastActionTime = Time.time;
                    AutoHidden = false;
                }
            }
        }
    }
}
