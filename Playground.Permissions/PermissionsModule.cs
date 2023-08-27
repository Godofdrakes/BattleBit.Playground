using System;
using System.Collections.Generic;
using System.Linq;
using BBRAPIModules;
using Playground.Common;

namespace Playground.Permissions;

public class ServerRoleDefinition
{
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public List<string> Permissions { get; set; } = new();
}

public class ServerRoleAssignment
{
	// the specific playerId to give this role to
	public ulong PlayerId { get; set; } = 0;
	public List<string> Roles { get; set; } = new();
}

public class PermissionsConfig : ModuleConfiguration
{
	public List<ServerRoleDefinition> Roles { get; set; } = new();
	public List<ServerRoleAssignment> Players { get; set; } = new();
}

public class PermissionsModule : BattleBitModule, IPermissionsModule
{
	public PermissionsConfig Config { get; set; } = new();

	private readonly Dictionary<string, ServerRoleDefinition> _serverRoles = new();
	private readonly Dictionary<ulong, HashSet<string>> _playerPermissions = new();

	public override void OnModulesLoaded()
	{
		// todo: enforce some kind of naming convention?

		foreach (var role in Config.Roles)
		{
			_serverRoles[role.Name] = role;
		}

		foreach (var roleAssignment in Config.Players)
		{
			if (!_playerPermissions.TryGetValue(roleAssignment.PlayerId, out var set))
			{
				_playerPermissions[roleAssignment.PlayerId] = set = new HashSet<string>();
			}

			foreach (var permission in roleAssignment.Roles.SelectMany(GetPermissionsForRole))
			{
				set.Add(permission);
			}
		}
	}

	public bool HasPermission(ulong playerId, string permission)
	{
		var permissions = _playerPermissions?[playerId] ?? Enumerable.Empty<string>();
		if (permissions.Contains(permission, StringComparer.InvariantCultureIgnoreCase))
		{
			return true;
		}

		return false;
	}

	public IEnumerable<string> GetPlayerPermissions(ulong playerId)
	{
		return _playerPermissions?[playerId] ?? Enumerable.Empty<string>();
	}

	public IEnumerable<string> GetPermissionsForRole(string role)
	{
		if (_serverRoles.TryGetValue(role, out var definition))
		{
			return definition.Permissions;
		}

		return Enumerable.Empty<string>();
	}
}
