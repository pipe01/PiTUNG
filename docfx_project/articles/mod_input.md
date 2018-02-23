# Capture keyboard input in a mod

There is an input system ([`ModInput`](../api/PiTung.ModInput.html)) meant to replace Unity's own `Input` class. If you know how to use Unity's `Input`, getting to know `ModInput` will be very easy. Here's an example on how to use it:

```
public class MyMod : Mod
{
	//...

	public override void BeforePatch()
	{
		//Register a binding called "TestKey" whose default key will be 'G'.
		//Then, fire methods when the key is pressed or released, and every frame that the key is down.
		ModInput.RegisterBinding(this, "TestKey", KeyCode.G)
			.ListenKeyDown(TestKeyDown)
			.ListenKey(TestKeyRepeat)
			.ListenKeyUp(TestKeyUp);
	}

	private void TestKeyDown()
	{
		IGConsole.Log("G key has just been pressed!");
	}

	private void TestKeyRepeat()
	{
		IGConsole.Log("G key is down!");
	}

	private void TestKeyUp()
	{
		IGConsole.Log("G key has just been released!");
	}
}



```

Alternatively, you may use the `ModInput.GetKey`, `ModInput.GetKeyDown` and `ModInput.GetKeyUp` methods just like Unity's `Input.GetKey...` methods, but instead of passing it a key code directly, you pass it the name of the key binding you registered with `ModInput.RegisterBinding`. Example:

```
public class MyMod : Mod
{
	//...

	public override void BeforePatch()
	{
		ModInput.RegisterBinding(this, "TestKey", KeyCode.G);
	}

	public override void Update()
	{
		if (ModInput.GetKeyDown("TestKey"))
		{
			IGConsole.Log("G key has just been pressed!");
		}
	}
}
```
