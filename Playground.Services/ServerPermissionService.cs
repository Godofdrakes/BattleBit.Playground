using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Playground.Common;

namespace Playground.Services;

public class ServerPermissionService :
	IHostedService,
	IServerPermissionService
{
	private readonly ILogger<ServerPermissionService> _logger;
	private readonly IConfiguration _configuration;
	private readonly IServiceProvider _serviceProvider;

	private Dictionary<string, ServerPermission> ServerPermissions { get; } = new();
	private Dictionary<string, ServerRole> ServerRoles { get; } = new();
	private Dictionary<ulong, HashSet<string>> PlayerPermissions { get; } = new();

	public ServerPermissionService(
		ILogger<ServerPermissionService> logger,
		IConfiguration configuration,
		IServiceProvider serviceProvider)
	{
		_logger = logger;
		_configuration = configuration;
		_serviceProvider = serviceProvider;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		var serverPermissions = _configuration
			.GetSection(nameof(ServerPermissionService))
			.GetSection(nameof(ServerPermissions))
			.GetChildren()
			.Select(section => section.Get<ServerPermission>())
			.WhereNotNull();

		foreach (var permission in serverPermissions)
		{
			if (!RegisterPermission(permission))
			{
				_logger.LogError("Failed to register permission");
			}
		}

		var serverRoles = _configuration
			.GetSection(nameof(ServerPermissionService))
			.GetSection(nameof(ServerRoles))
			.GetChildren()
			.Select(section => section.Get<ServerRole>())
			.WhereNotNull();

		foreach (var role in serverRoles)
		{
			if (!RegisterRole(role))
			{
				_logger.LogError("Failed to register role");
			}
		}

		var allPlayerRoles = _configuration
			.GetSection(nameof(ServerPermissionService))
			.GetSection(nameof(PlayerRoles))
			.GetChildren()
			.Select(section => section.Get<PlayerRoles>())
			.WhereNotNull();

		foreach (var playerRoles in allPlayerRoles)
		{
			if (!RegisterPlayerRoles(playerRoles))
			{
				_logger.LogError("Failed to register one or more player roles");
			}
		}

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
	
	private bool RegisterPermission(ServerPermission serverPermission)
	{
		// todo: enforce no whitespace in names

		if (!ServerPermissions.TryAdd(serverPermission.Name, serverPermission))
		{
			_logger.LogError("ServerPermission '{Name}' is already registered", serverPermission.Name);
			return false;
		}

		return true;
	}

	private bool RegisterRole(ServerRole serverRole)
	{
		// todo: enforce no whitespace in names

		if (!ServerRoles.TryAdd(serverRole.Name, serverRole))
		{
			_logger.LogError("ServerRole '{Name}' is already registered", serverRole.Name);
			return false;
		}

		return true;
	}
	
	private bool RegisterPlayerRoles(PlayerRoles playerRoles)
	{
		if (playerRoles.PlayerId == 0)
		{
			_logger.LogError("Invalid playerId");
			return false;
		}

		foreach (var roleName in playerRoles.Roles)
		{
			if (!ServerRoles.TryGetValue(roleName, out var role))
			{
				_logger.LogError("Unknown role '{Name}'", roleName);
				return false;
			}

			_logger.LogDebug("Assigning {Player} to role '{Role}'",
				playerRoles.PlayerId, roleName);

			HashSet<string>? permissionsSet;

			if (!PlayerPermissions.TryGetValue(playerRoles.PlayerId, out permissionsSet))
			{
				PlayerPermissions[playerRoles.PlayerId] = permissionsSet = new HashSet<string>();
			}

			foreach (var permission in role.Permissions)
			{
				permissionsSet.Add(permission);
			}
		}

		return true;
	}

	public IEnumerable<string> GetPermissions(ulong playerId)
	{
		if (PlayerPermissions.TryGetValue(playerId, out var permissionsSet))
		{
			return permissionsSet.AsEnumerable();
		}

		return Enumerable.Empty<string>();
	}

	public bool HasPermission(ulong playerId, string permission)
	{
		if (PlayerPermissions.TryGetValue(playerId, out var permissionsSet))
		{
			return permissionsSet.Contains(permission);
		}

		return false;
	}
}