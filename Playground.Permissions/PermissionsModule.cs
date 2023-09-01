using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BBRAPIModules;
using Playground.Common;

namespace Playground.Permissions;

public class PermissionsConfig : ModuleConfiguration
{
	public Dictionary<ulong, HashSet<string>> PlayerPermissions { get; set; } = new();
}

public class PermissionsModule : BattleBitModule, IPermissionsModule
{
	[ModuleReference]
	public ICommandsModule? CommandsModule { get; set; }

	public PermissionsConfig Config { get; set; } = new();

	private readonly TextWriter _logger;
	
	public PermissionsModule()
	{
		_logger = Console.Out;
	}

	public override void OnModulesLoaded()
	{
		CommandsModule?.Register(this);
	}

	public override void OnModuleUnloading()
	{
		CommandsModule?.Unregister(this);
	}

	public void AddPlayerPermission(ulong playerId, string permission)
	{
		_logger.WriteLine($"Adding permission {permission} to player {playerId}");

		lock (Config)
		{
			Config.PlayerPermissions.TryGetValue(playerId, out var permissionSet);

			if (permissionSet is null)
			{
				Config.PlayerPermissions[playerId] = permissionSet = new HashSet<string>();
			}

			if (permissionSet.Add(permission))
			{
				Config.Save();
			}
		}
	}

	public void RemovePlayerPermission(ulong playerId, string permission)
	{
		_logger.WriteLine($"Removing permission {permission} from player {playerId}");

		lock (Config)
		{
			Config.PlayerPermissions.TryGetValue(playerId, out var permissionSet);

			if (permissionSet is not null)
			{
				if (permissionSet.Remove(permission))
				{
					Config.Save();
				}
			}
		}
	}

	public bool HasPermission(ulong playerId, string? permission)
	{
		if (permission is null)
		{
			return true;
		}

		lock (Config)
		{
			Config.PlayerPermissions.TryGetValue(playerId, out var permissionSet);

			if (permissionSet is not null)
			{
				return permissionSet.Contains(permission);
			}
		}

		return false;
	}

	public IEnumerable<string> GetPlayerPermissions(ulong playerId)
	{
		lock (Config)
		{
			Config.PlayerPermissions.TryGetValue(playerId, out var permissionSet);

			if (permissionSet is not null)
			{
				return permissionSet.ToArray();
			}
		}

		return Enumerable.Empty<string>();
	}

	[Command("perm reload")]
	private void ReloadPermissionsCommand()
	{
		_logger.WriteLine("Reloading permissions");

		lock (Config)
		{
			Config.Load();
		}
	}
}
