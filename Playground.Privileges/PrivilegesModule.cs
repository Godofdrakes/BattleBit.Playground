using BBRAPIModules;

namespace Playground.Privileges;

public class ServerPrivilege { }

public class ServerRole { }

public class ServerRollCollection
{
	public bool HasPrivilege(string key)
	{
		throw new NotImplementedException();
	}
}

public class PrivilegesModule : BattleBitModule
{
	private Dictionary<ulong, ServerRollCollection> PlayerRoles { get; } = new();

	public bool HasPrivilege(RunnerPlayer player, string key)
	{
		if (PlayerRoles.TryGetValue(player.SteamID, out var roles))
		{
			return roles.HasPrivilege(key);
		}

		return false;
	}
}
