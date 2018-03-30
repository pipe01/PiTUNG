using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace PiTung.Components
{
    internal class CustomMenu
    {
        public static CustomMenu Instance { get; } = new CustomMenu();

        private const int FontSize = 15;

        private GUIStyle NormalStyle, SelectedStyle;
        
        public bool Visible { get; set; }
        public int Selected { get; private set; }
        public bool SelectionChanged { get; private set; }

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
                //wordWrap = true
            };

            SelectedStyle = new GUIStyle(NormalStyle)
            {
                normal = new GUIStyleState
                {
                    background = ModUtilities.Graphics.CreateSolidTexture(1, 1, new Color(0, 0.3f, 0, 0.3f)),
                    textColor = Color.cyan
                }
            };
        }

        public void Draw()
        {
            if (ModUtilities.IsOnMainMenu || !Visible)
                return;

            BeginArea(new Rect(40, 40, 100, 400));
            {
                int i = 0;
                foreach (var item in ComponentRegistry.Registry.Values)
                {
                    Label(item.DisplayName, i++ == Selected ? SelectedStyle : NormalStyle);
                }
            }
            EndArea();
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

                IGConsole.Log(Selected);

                if (Selected == ComponentRegistry.Registry.Count)
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
                }
            }
        }
    }
}
