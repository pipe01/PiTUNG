using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace PiTung.User_Interface
{
    internal class ModsScreen
    {
        public static ModsScreen Instance { get; } = new ModsScreen();

        private static bool HiddenCanvases = true;
        private static IList<Canvas> WereVisible = new List<Canvas>();
        private static IList<Mod> UnloadedMods = new List<Mod>();

        private bool _visible;
        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;

                SetGameUIVisible(!_visible);
            }
        }

        private Vector2 Scroll = Vector2.zero;
        private Rect BoxRect;
        private GUIStyle BackStyle, UnloadedBackStyle, DetailsStyle;

        private ModsScreen()
        {
            BackStyle = new GUIStyle
            {
                normal = new GUIStyleState
                {
                    background = ModUtilities.Graphics.CreateSolidTexture(1, 1, new Color(0, 0, 0, 0.4f))
                },
                margin = new RectOffset(3, 0, 0, 0)
            };

            UnloadedBackStyle = new GUIStyle(BackStyle)
            {
                normal = new GUIStyleState
                {
                    background = ModUtilities.Graphics.CreateSolidTexture(1, 1, new Color(0.3f, 0, 0, 0.4f))
                }
            };

            DetailsStyle = new GUIStyle(GUI.skin.label)
            {
                normal = new GUIStyleState
                {
                    textColor = new Color(0.6f, 0.6f, 0.6f)
                }
            };
        }
        
        public void Draw()
        {
            if (!Visible)
            {
                if (GUI.Button(new Rect(155, 5, 55, 30), "Mods") && RunMainMenu.Instance.MainMenuCanvas.enabled)
                {
                    Visible = true;
                }
            }
            else
            {
                DrawScreen();
            }
        }

        private void DrawScreen()
        {
            if (GUI.Button(new Rect(Screen.width - 200, Screen.height - 100, 150, 50), "Back") || (Event.current.isKey && Event.current.keyCode == KeyCode.Escape))
            {
                Visible = false;
            }

            float width = 300;
            BoxRect = new Rect(width, 100, Screen.width - (width * 2), Screen.height - 200);

            GUI.Box(BoxRect, "Mods");

            BeginArea(BoxRect);
            {
                Space(20);

                Scroll = BeginScrollView(Scroll, false, true);
                {
                    foreach (var item in Bootstrapper._Mods
                            .Concat(UnloadedMods)
                            .OrderBy(o => UnloadedMods.Contains(o))
                            .ThenBy(o => o.Name))
                    {
                        DrawMod(item);
                    }

                    FlexibleSpace();
                }
                EndScrollView();
            }
            EndArea();
        }

        private void DrawMod(Mod mod)
        {
            bool unloaded = UnloadedMods.Contains(mod);

            BeginHorizontal(unloaded ? UnloadedBackStyle : BackStyle, Height(63));
            {
                BeginVertical();
                {
                    Label($"<size=20>{mod.Name}</size>");

                    Label("by " + mod.Author, DetailsStyle);
                }
                EndVertical();

                FlexibleSpace();

                BeginVertical();
                {
                    BeginHorizontal();
                    {
                        FlexibleSpace();

                        Label(mod.PackageName, DetailsStyle);
                    }
                    EndHorizontal();

                    FlexibleSpace();

                    BeginHorizontal();
                    {
                        FlexibleSpace();

                        if ((!unloaded || mod.Hotloadable) && Button(unloaded ? "Reload" : "Unload", Width(60), Height(30)))
                        {
                            if (unloaded)
                            {
                                var newMod = ModLoader.GetMod(mod.FullPath);

                                Bootstrapper.Instance.LoadMod(newMod, false);

                                UnloadedMods.Remove(mod);
                            }
                            else
                            {
                                Bootstrapper.Instance.UnloadMod(mod);
                                mod.ModAssembly = null;

                                UnloadedMods.Add(mod);
                            }
                        }
                    }
                    EndHorizontal();
                }
                EndVertical();
            }
            EndHorizontal();
            
            Space(3);
        }

        private static void SetGameUIVisible(bool visible)
        {
            if (HiddenCanvases == visible)
                return;
            HiddenCanvases = visible;

            var i = RunMainMenu.Instance;

            SetVisible(i.AboutCanvas, visible);
            SetVisible(i.DeleteGameCanvas, visible);
            SetVisible(i.LoadGameCanvas, visible);
            SetVisible(i.MainMenuCanvas, visible);
            SetVisible(i.NewGameCanvas, visible);
            SetVisible(i.OptionsCanvas, visible);
            SetVisible(i.RenameGameCanvas, visible);

            void SetVisible(Canvas v, bool vis)
            {
                if (v.enabled && !vis)
                {
                    WereVisible.Add(v);
                    v.enabled = false;
                }
                else if (vis && WereVisible.Contains(v))
                {
                    WereVisible.Remove(v);
                    v.enabled = true;
                }
            }
        }
    }
}
