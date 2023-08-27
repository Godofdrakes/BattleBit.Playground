namespace Playground.Common;

public interface IServerPermissionService
{
	public IEnumerable<string> GetPermissions(ulong playerId);

	public bool HasPermission(ulong playerId, string permission)
	{
		return GetPermissions(playerId).Contains(permission);
	}
}