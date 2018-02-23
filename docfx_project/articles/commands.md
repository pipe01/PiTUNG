# Using console commands

To register your own command for your mod, you must use the [`IGConsole.RegisterCommand<T>(Mod)`][1] method (you may alternatively use the [`IGConsole.RegisterCommand(Command, Mod)`][2] method if your command needs constructor parameters). The generic type parameter (`<T>`) must be a reference to your class that inherits from [`Command`][3]. The parameter of type `Mod` must be a reference to your mod (`this` in most cases). 

Your command class must override the [`Name`][4] and [`Usage`][5] properties, as well as the [`Execute(IEnumerable<string>)`][6] method. It may also optionally override the [`Description`][7] property, which contains a string that will be displayed alongside the command name when the `help` command is run.

Example command class:

```
public class Command_mycmd : Command
{
	public override string Name { get; } => "mycmd";
	public override string Usage { get; } => $"{Name} first_argument [optional_argument]";
	public override string Description { get; } => "This is my test command, it does nothing!";

	public override bool Execute(IEnumerable<string> args)
	{
		if (args.Count == 0)
			return false;

		IGConsole.Log("This is my command!");

		return true;
	}
}
```

Registering it:

```
public class MyMod : Mod
{
	//...

	public override void BeforePatch()
	{
		IGConsole.RegisterCommand<Command_mycmd>(this);
	}
}
```

[1]: ../api/PiTung.Console.IGConsole.html#PiTung_Console_IGConsole_RegisterCommand__1_PiTung_Mod_
[2]: ../api/PiTung.Console.IGConsole.html#PiTung_Console_IGConsole_RegisterCommand_PiTung_Console_Command_PiTung_Mod_
[3]: ../api/PiTung.Console.Command.html
[4]: ../api/PiTung.Console.Command.html#PiTung_Console_Command_Name
[5]: ../api/PiTung.Console.Command.html#PiTung_Console_Command_Usage
[6]: ../api/PiTung.Console.Command.html#PiTung_Console_Command_Execute_System_Collections_Generic_IEnumerable_System_String__
[7]: ../api/PiTung.Console.Command.html#PiTung_Console_Command_Description