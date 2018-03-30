# Custom components

There are two parts for each custom component: the "structure" ([`PrefabBuilder`][1]), and the logic ([`UpdateHandler`][2]). For the structure, you've got two choices: [`PrefabBuilder.Cube`][3], and [`PrefabBuilder.Custom()`][4].

## Cube

This is the easiest shape and it's also the shape of the usual TUNG components (inverter, blotter, etc). You can set [5 sides][5] of a cube (the bottom side can't be set for obvious reasons) to be either an input, an output, or neither. For example, say that we wanted to create a component similar to an inverter:

```C#
...
	public override void BeforePatch()
	{
		var cube = PrefabBuilder
			.Cube
			.SetSide(CubeSide.Top, SideType.Input)
			.SetSide(CubeSide.Front, SideType.Output);

		ComponentRegistry.CreateNew<MyInverterHandler>("inverter", cube);
	}
...
```

The `MyInverterHandler` class will be shown later.

## Custom shape

If you want more flexibility, you can use your own `GameObject` prefab as your component root. For example:

```C#
...
	public override void BeforePatch()
	{
		var custom = PrefabBuilder
			.Custom(() => GameObject.CreatePrimitive(PrimitiveType.Capsule))
			.AddInput(x, y, z)
			.AddOutput(x, y, z);

		ComponentRegistry.CreateNew<MyInverterHandler>("mycapsule", custom);
	}
...
```

Note that in order to create objects this way, you'll need an assembly reference to ```UnityEngine.CoreModule``` (found in ```The Ultimate Nerd Game_Data\Managed```)

# Update handlers

Once you have designed how your component should look, you must decide how it should act. That's where the `MyInverterHandler` class comes into play. A component's behaviour is defined by a [`UpdateHandler`][6] class. That class derives from `MonoBehaviour`, which means that you will be able to use Unity to its full potential.

Note that in order to use `Monobehaiour`, you will have to add a project reference to UnityEngine.CoreModule. This dll is found in `The Ultimate Nerd Game_Data\Managed`.

When the game fires an update tick, the `UpdateHandler.CircuitLogicUpdate` method will be called. Keep in mind that this method won't always be called, but only when you have called `QueueCircuitLogicUpdate` on it (which will add the component to the aforementioned queue), or when any of its inputs gets changed.

You can access the component's inputs and outputs via the [`Inputs`][7] and [`Outputs`][8] properties, repectively. For example:

```C#
public class MyInverterHandler : UpdateHandler
{
	protected override void CircuitLogicUpdate()
	{
		this.Outputs[0].On = !this.Inputs[0].On;
	}
}
```

## Component data

Your component can have fields marked with [`[SaveThis]`][9]. Those fields will be saved and loaded along with the world save, which means that you'll be able to save your own data.

[1]: ../api/PiTung.Components.PrefabBuilder.html
[2]: ../api/PiTung.Components.UpdateHandler.html
[3]: ../api/PiTung.Components.PrefabBuilder.html#PiTung_Components_PrefabBuilder_Cube
[4]: ../api/PiTung.Components.PrefabBuilder.html#PiTung_Components_PrefabBuilder_Custom_System_Func_UnityEngine_GameObject__
[5]: ../api/PiTung.Components.CubeSide.html
[6]: ../api/PiTung.Components.UpdateHandler.html
[7]: ../api/PiTung.Components.UpdateHandler.html#PiTung_Components_UpdateHandler_Inputs
[8]: ../api/PiTung.Components.UpdateHandler.html#PiTung_Components_UpdateHandler_Outputs
[9]: ../api/PiTung.Components.SaveThisAttribute.html
