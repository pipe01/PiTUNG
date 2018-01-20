# PiTung
Mod framework for The Ultimate Nerd Game

# How to create a mod

This is a barebones mod class:
```C#
using PiTung_Bootstrap;
using System;
using UnityEngine;

public class MyMod : Mod
{
    public override string ModName => "Your mod name goes here";
    public override string ModAuthor => "your name";
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
