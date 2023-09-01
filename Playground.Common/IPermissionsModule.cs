using System.Collections.Generic;
using BBRAPIModules;

namespace Playground.Common;

public interface IPermissionsModule
{
	bool HasPermission(ulong playerId, string? permission);

	IEnumerable<string> GetPlayerPermissions(ulong playerId);

	bool HasPermission(RunnerPlayer player, string? permission)
	{
		return HasPermission(player.SteamID, permission);
	}

	IEnumerable<string> GetPlayerPermissions(RunnerPlayer player)
	{
		return GetPlayerPermissions(player.SteamID);
	}
}