using System.Net;

namespace Playground.Console;

public class ServerConfig
{
	// the default port, as per https://github.com/MrOkiDoki/BattleBit-Community-Server-API
	public const int PORT_DEFAULT = 29294;

	// the local address to listen for server connections on
	public IPAddress Address { get; set; } = IPAddress.Any;

	// the local port ot listen for server connections on
	public int Port { get; set; } = PORT_DEFAULT;

	public List<string> Modules { get; set; } = new();
}