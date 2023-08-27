using System.Net;
using BattleBitAPI.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Playground.Common;

namespace Playground.Services;

public class ServerListenerService : IHostedService
{
	private readonly ILogger<ServerListenerService> _logger;
	private readonly IConfiguration _configuration;
	private readonly IServiceProvider _serviceProvider;

	private ServerListener<GamePlayer, GameServer>? _serverListener;

	public ServerListenerService(
		ILogger<ServerListenerService> logger,
		IConfiguration configuration,
		IServiceProvider serviceProvider)
	{
		_logger = logger;
		_configuration = configuration;
		_serviceProvider = serviceProvider;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogDebug(nameof(StartAsync));

		var config = _configuration
			.GetSection(nameof(ServerListenerService))
			.GetSection(nameof(ServerConfig))
			.Get<ServerConfig>();

		config ??= new ServerConfig();

		_serverListener = new ServerListener<GamePlayer, GameServer>();

		// _serverListener.Start(config.Address, config.Port);
		_serverListener.OnCreatingGameServerInstance += OnCreatingGameServerInstance;
		_serverListener.OnCreatingPlayerInstance += OnCreatingPlayerInstance;

		return Task.CompletedTask;
	}

	private GameServer OnCreatingGameServerInstance(IPAddress address, ushort port)
	{
		_logger.LogDebug(nameof(OnCreatingGameServerInstance));

		return ActivatorUtilities.CreateInstance<GameServer>(_serviceProvider);
	}

	private GamePlayer OnCreatingPlayerInstance(ulong steamId)
	{
		_logger.LogDebug(nameof(OnCreatingPlayerInstance));

		return ActivatorUtilities.CreateInstance<GamePlayer>(_serviceProvider);
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogDebug(nameof(StopAsync));

		_serverListener?.Dispose();
		_serverListener = null;

		return Task.CompletedTask;
	}
}