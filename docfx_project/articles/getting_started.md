# Getting started

So, you want to develop a mod for TUNG! That's awesome! Fortunately, with PiTUNG it's very easy to create a simple mod.

## Setting up the environment

First, you must create a Visual Studio project. Once it's loaded, go to its properties and set the target framework version to `.NET 3.5`.

Then, you will need a PiTUNG binary. There are three ways of getting ahold of one:

### Downloading the latest stable release

Go to [the latest release in GitHub](https://github.com/pipe01/PiTUNG/releases/latest) and download the `PiTUNG Bootstrap.dll` file.

### Downloading the latest unstable release

This is the version compiled from the latest commit to the GitHub repo, so it will probably be unstable and should only be used when you want to test out some new features, but never in production.

To get it, simply download the [artifact from AppVeyor](https://ci.appveyor.com/api/projects/pipe01/pitung/artifacts/bin%2FDebug%2FPiTung%20Bootstrap.dll).

### Compiling from source
//TODO

---

Now that you've got a PiTUNG binary:

1. Download a copy of TUNG either [here](https://iamsodarncool.itch.io/tung) or [here](https://gamejolt.com/games/tung/304428) and extract it to a folder.
2. Install PiTUNG as you normally would.
3. Go to `{install folder}\The Ultimate Nerd Game_Data\Managed` and copy the `Assembly-CSharp.dll` file to wherever you want, take note of its location.
4. Add `PiTUNG Bootstrap.dll` and `Assembly-CSharp.dll` to your project's references.
5. Create a new file called `MyMod.cs` and replace its content with this code:

    ```
    using PiTung_Bootstrap;

    public class MyMod : Mod
    {
        public override string Name => "Example mod";
        public override string PackageName => "me.myname.ExampleMod";
        public override string Author => "john_doe";
        public override Version ModVersion => new Version("0.1");
        public override Version FrameworkVersion => new Version("1.3");
    }
    ```

6. Compile it and copy the output DLL file to `{TUNG installation}/mods`.
7. Congratulations, now you've got a mod that does absolutely nothing!

You can now take a look at [the API documentation](../api/index.html) for more information about the API.