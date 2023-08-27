using BBRAPIModules;

namespace Playground.Common;

public static class RunnerPlayerEx
{
	public static bool HasPermission(this RunnerPlayer player, IPermissionsModule? permissionsModule, string permission, bool bDefault = false)
	{
		return permissionsModule?.HasPermission(player.SteamID, permission) ?? bDefault;
	}
}