# PiTung
[![Build status](https://ci.appveyor.com/api/projects/status/9v5a2adge9t2ysxa?svg=true)](https://ci.appveyor.com/project/pipe01/pitung)

Mod framework for The Ultimate Nerd Game

# For users: How to install the framework

Installing the framework is extremely easy, just download [the installer](http://www.pipe0481.heliohost.org/pitung/Installer.exe) to the same folder as "The Ultimate Nerd Game.exe", and double-click it!

# For developers: How to create a mod

This is a simple example mod class:
```C#
using PiTung_Bootstrap;
using System;
using UnityEngine;

public class MyMod : Mod
{
    public override string Name => "Your mod name goes here";
    public override string Author => "your name";
    public override Version ModVersion => new Version(1, 0, 0);

    protected override KeyCode[] ModKeys => new[] { KeyCode.O };

    private static bool Pressed;

    public override void OnKeyDown(KeyCode keyCode)
    {
        Pressed = true;
    }
    
    public override void OnGUI()
    {
        ModUtilities.Graphics.DrawText("Hello world!", new Vector2(5, 5), Color.green);

        if (Pressed)
            ModUtilities.Graphics.DrawText("You pressed O", new Vector2(5, 15), Color.green);
    }
    
    [Postfix(typeof(BehaviorManager), "Awake")]
    public static void BehaviorManagerAwake(BehaviorManager __instance)
    {
        ModUtilities.Log("Hello debug!");
    }
}
```

This framework uses [Harmony](https://github.com/pardeike/Harmony) under the hood, so you may want to take a look at its documentation if you need more flexibility.
