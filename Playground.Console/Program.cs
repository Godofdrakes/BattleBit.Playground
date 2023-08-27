using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground.Services;

namespace Playground.Console;

public static class Program
{
	public static Task Main(string[] args)
	{
		var hostBuilder = Host.CreateDefaultBuilder(args);

#if DEBUG
		hostBuilder.UseEnvironment(Environments.Development);
#endif

		var host = hostBuilder
			.ConfigureServerListener()
			.UseConsoleLifetime()
			.Build();

		return host.RunAsync();
	}

	private static IHostBuilder ConfigureServerListener(this IHostBuilder hostBuilder)
	{
		return hostBuilder.ConfigureServices(services =>
		{
			services.AddHostedService<ServerListenerService>();
		});
	}
}