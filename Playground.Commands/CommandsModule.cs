using System;
using System.Collections.Generic;
using System.Reflection;
using BBRAPIModules;
using Playground.Common;

namespace Playground.Commands;

public class CommandsConfig : ModuleConfiguration
{
	public string CommandPrefix { get; set; } = "!";
}

public class CommandEntry
{
	public BattleBitModule Module { get; }
	public MethodInfo MethodInfo { get; }

	public CommandEntry(BattleBitModule module, MethodInfo methodInfo)
	{
		Module = module;
		MethodInfo = methodInfo;
	}
}

public class CommandsModule : BattleBitModule, ICommandsModule
{
	public CommandsConfig Config { get; set; } = new();

	private readonly Dictionary<string, CommandEntry> _commandEntries = new();

	public void Register(BattleBitModule module)
	{
		throw new NotImplementedException();
	}
}