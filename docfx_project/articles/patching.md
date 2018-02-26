# How to modify the game's code

This framework is based on the ability to patch the game's methods, thus modifying the game's logic. Before you modify any method, you must first know which method to modify! This is why you must decompile the game's `Assembly-CSharp.dll` file, which contains the game's logic. I can't go into detail on how to do this, but it shouldn't be too hard to google it.

After you've selected the method you want to patch, you must create a class inside your mod with a [`TargetAttribute`][1] attribute, which indicates the class that contains the method you want to patch. Inside your class, you must define a method (or more) with the same name as the one you want to patch and with a [`PatchMethodAttribute`][2] attribute.

There are two kinds of method patches: prefix and postfix. As their name indicates, a prefix patch method will get executed before the original method does, and a postfix patch method will get executed after the original method is executed. You can indicate what kind of patch you want via the `PatchMethodAttribute.PatchType` parameter.

For example, this will execute some code before the method `MyClass.MyMethod`:

```
[Target(typeof(MyClass))]
static class MyClassPatch
{
	[PatchMethod]
	static void MyMethod()
	{
		IGConsole.Log("MyMethod will be executed!");
	}
}
```

[1]: ../api/PiTung.TargetAttribute.html
[2]: ../api/PiTung.PatchMethodAttribute.html