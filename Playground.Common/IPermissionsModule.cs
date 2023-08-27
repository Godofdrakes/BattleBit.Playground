using System.Collections.Generic;
using BBRAPIModules;

namespace Playground.Common;

public static class PermissionsModuleEx
{
	public static bool HasPermission(this IPermissionsModule? permissionsModule, RunnerPlayer player, string permission)
	{
		if (permissionsModule is not null)
		{
			return permissionsModule.HasPermission(player.SteamID, permission);
		}

		return true;
	}
}

public interface IPermissionsModule
{
	bool HasPermission(ulong playerId, string permission);

	IEnumerable<string> GetPlayerPermissions(ulong playerId);
}