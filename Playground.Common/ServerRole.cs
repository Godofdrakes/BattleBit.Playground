namespace Playground.Common;

public class ServerRole
{
	public string Name { get; set; } = string.Empty;
	public List<string> Permissions { get; set; } = new();
}