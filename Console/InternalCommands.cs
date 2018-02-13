using System.Security.Permissions;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using static PiTung_Bootstrap.Console.IGConsole;
using PiTung_Bootstrap.Building;
using System.Reflection;

namespace PiTung_Bootstrap.Console
{
    internal class Command_help : Command
    {
        public override string Name => "help";
        public override string Usage => $"{Name} [command]";
        public override string Description => "Lists command and shows their usage (help command)";

        public override bool Execute(IEnumerable<string> arguments)
        {
            if (!arguments.Any())
            {
                var cmds = from o in Registry.Values
                           where o.ShowOnHelp
                           orderby o.Name
                           select o;

                Log("<color=orange>PiTUNG</color>:");
                foreach (var cmd in cmds.Where(o => o.Mod == null))
                {
                    Log(GetCommandLine(cmd));
                }

                Mod prevMod = null;
                foreach (var cmd in cmds.Where(o => o.Mod != null).OrderBy(o => o.Mod.Name))
                {
                    if (prevMod != cmd.Mod)
                    {
                        prevMod = cmd.Mod;
                        Log($"\n<color=orange>{cmd.Mod.Name}</color>:");
                    }

                    Log(GetCommandLine(cmd));
                }
            }
            else if (arguments.Count() == 1)
            {
                string name = arguments.ElementAt(0);
                Command command;
                if (Registry.TryGetValue(name, out command))
                {
                    Log(command.Description);
                    Log("<b>Usage:</b> " + command.Usage);
                }
                else
                {
                    Error($"No such command \"{name}\"");
                }
            }
            else
            {
                return false;
            }

            return true;

            string GetCommandLine(Command command)
            {
                string log = $"<b>{command.Name}</b>";
                if (command.Description != null)
                    log += ": " + command.Description;
                return log;
            }
        }
    }

    internal class Command_mods : Command
    {
        public override string Name => "mods";
        public override string Usage => Name;
        public override string Description => "Lists loaded mods";

        public override bool Execute(IEnumerable<string> arguments)
        {
            Log($"<b>Loaded mods ({Bootstrapper.ModCount}):</b> " + String.Join(", ", Mod.AliveMods.Select(o => $"'{o.FullName}'").ToArray()));
            
            return true;
        }
    }

    internal class Command_set : Command
    {
        public override string Name => "set";
        public override string Usage => $"{Name} variable [value]";
        public override string Description => "Sets global variables";

        public override bool Execute(IEnumerable<string> arguments)
        {
            string value = null;
            string variable = arguments.FirstOrDefault();

            if (arguments.Count() == 1)
            {
                //Clear variable
                value = "";
            }
            else if (arguments.Count() == 2)
            {
                value = arguments.ElementAt(1);
            }
            else
            {
                return false;
            }

            SetVariable(variable, value);
            return true;
        }
    }

    internal class Command_get : Command
    {
        public override string Name => "get";
        public override string Usage => $"{Name} variable";
        public override string Description => "Gets global variables";

        public override bool Execute(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 1)
            {
                string variable = arguments.First();
                string value = GetVariable(variable);

                if (value != null)
                {
                    Log($"\"{variable}\" = \"{value}\"");
                }
                else
                {
                    Log(LogType.ERROR, $"Variable \"{variable}\" not found.");
                }

                return true;
            }

            return false;
        }
    }

    internal class Command_reload : Command
    {
        public override string Name => "reload";
        public override string Usage => Name;
        public override string Description => "Tries to load new mods";

        public override bool Execute(IEnumerable<string> arguments)
        {
            int oldCount = Bootstrapper.ModCount;

            Bootstrapper.Instance.Patch(true);

            int newMods = Bootstrapper.ModCount - oldCount;

            Log($"Done. {newMods} new mods loaded.");

            return true;
        }
    }

    internal class Command_echo : Command
    {
        public override string Name => "echo";
        public override string Usage => $"{Name} text";
        public override string Description => "Prints text to the console.";

        public override bool Execute(IEnumerable<string> arguments)
        {
            string str = string.Join(" ", arguments.ToArray());

            Log(str);

            return true;
        }
    }

    internal class Command_quit : Command
    {
        public override string Name => "quit";
        public override string Usage => Name;
        public override string Description => "Quits the game.";

        public override bool Execute(IEnumerable<string> arguments)
        {
            Application.Quit();
            return true;
        }
    }

    internal class Command_objs : Command
    {
        public override string Name => "objs";
        public override string Usage => Name;
        internal override bool ShowOnHelp => false;

        public override bool Execute(IEnumerable<string> arguments)
        {
            var scene = SceneManager.GetActiveScene();

            foreach (var obj in scene.GetRootGameObjects())
            {
                Recurse(obj);
            }

            return true;

            void Recurse(GameObject parent, int level = 0)
            {
                string compStr = string.Join(", ", parent.GetComponents<Component>().Select(o => o.GetType().Name).ToArray());
                
                MDebug.WriteLine(new string(' ', level * 4) + parent.name + $" ({compStr})");

                foreach (Transform item in parent.transform)
                {
                    Recurse(item.gameObject, level + 1);
                }
            }
        }
    }

    internal class Command_praise : Command
    {
        public override string Name => "praise";
        public override string Usage => "you know what to do";
        internal override bool ShowOnHelp => false;

        public override bool Execute(IEnumerable<string> arguments)
        {
            var args = arguments.ToArray();

            if (args.Length >= 2 && args[0] == "the" && args[1] == "sun")
            {
                Log("May your life be full of joy and happiness.");
                BoringStuff.SUMMONEXODIA();
            }
            else
            {
                Error("HERESY WILL NOT BE TOLERATED");
            }

            return true;
        }
    }


    internal class Command_test : Command
    {
        public override string Name => "test";
        public override string Usage => Name;

        public override bool Execute(IEnumerable<string> arguments)
        {
            switch (arguments.First())
            {
                case "getall":
                    int bid = int.Parse(arguments.ElementAt(1));

                    if (BoardManager.Instance.TryGetBoard(bid, out var bb))
                    {
                        foreach (var item in bb.GetComponents())
                        {
                            Log(item);
                        }
                    }

                    break;
                case "get":
                    int x = int.Parse(arguments.ElementAt(1));
                    int y = int.Parse(arguments.ElementAt(2));
                    int id = int.Parse(arguments.ElementAt(3));

                    if (BoardManager.Instance.TryGetBoard(id, out var b))
                    {
                        var c = b.GetComponentAt(x, y);
                        
                        if (c == null)
                        {
                            Error("Component not found.");
                            break;
                        }

                        if (c.name.StartsWith("CircuitBoard"))
                        {
                            if (BoardManager.Instance.TryGetExistingBoardFromGameObject(c, out var childBoard))
                            {
                                Log($"Board {childBoard.Width}x{childBoard.Height}");
                            }
                            else
                            {
                                Log("Board");
                            }
                            break;
                        }

                        foreach (var item in Components.GetComponentNames())
                        {
                            if (c.name.StartsWith(item))
                            {
                                Log(item);
                                break;
                            }
                        }

                    }
                    else
                    {
                        Error("Board not found");
                    }

                    break;
                case "board":
                    Assembly ass = Assembly.LoadFrom(@".\The Ultimate Nerd Game_Data\Managed\UnityEngine.PhysicsModule.dll");
                    var p = ass.GetType("UnityEngine.Physics");
                    
                    var transform = FirstPersonInteraction.FirstPersonCamera.transform;
                    var args = new object[] { transform.position, transform.forward, null, MiscellaneousSettings.ReachDistance };
                    bool raySuccess = (bool)p.InvokeMember("Raycast", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, args);
                    
                    if (raySuccess)
                    {
                        var t = args[2].GetType().GetProperty("transform").GetValue(args[2], null);
                        var obj = (GameObject)t.GetType().GetProperty("gameObject").GetValue(t, null);

                        if (BoardManager.Instance.TryGetExistingBoardFromGameObject(obj, out var bo))
                        {
                            Log($"Board {bo.Width}x{bo.Height} id: {bo.Id}");
                        }
                        else
                        {
                            Error("Board not registered");
                        }
                    }

                    break;
                case "ser":
                    int asdasdid = int.Parse(arguments.ElementAt(1));

                    if (BoardManager.Instance.TryGetExistingBoardFromGameObject(BoardManager.Instance.GetBoard(asdasdid).Object, out var baos))

                        Print(Serialize(baos));

                    break;
            }

            return true;

            void Print(BoardComp comp, int level = 0)
            {
                Log(new string('-', level * 2) + comp.Type);

                foreach (var item in comp.Children)
                {
                    Print(item, level + 1);
                }
            }
        }

        private class BoardComp
        {
            public string Type;
            public int X, Y;
            public List<BoardComp> Children = new List<BoardComp>();
            public GameObject Object;
        }

        private static BoardComp Serialize(Board board)
        {
            BoardComp parent = new BoardComp();
            parent.Type = "CircuitBoard";
            parent.Object = board.Object;
            
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    for (int side = 0; side < 2; side++)
                    {
                        var comp = board.GetComponentAt(x, y, side);

                        if (comp == null)
                            continue;

                        if (comp.name.StartsWith("CircuitBoard") &&
                            !parent.Children.Any(o => o.Object.GetInstanceID() == comp.GetInstanceID()) &&
                            BoardManager.Instance.TryGetExistingBoardFromGameObject(comp, out var child))
                        {
                            parent.Children.Add(Serialize(child));
                        }
                        else
                        {
                            foreach (var item in Components.GetComponentNames())
                            {
                                if (comp.name.StartsWith(item))
                                {
                                    parent.Children.Add(new BoardComp
                                    {
                                        Object = comp,
                                        X = x,
                                        Y = y,
                                        Type = item
                                    });

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return parent;
        }
    }
}
