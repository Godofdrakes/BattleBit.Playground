namespace Playground.Common;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
	public CommandAttribute(
		string name,
		string? description = default)
	{
		Name = name;
		Description = description ?? string.Empty;
	}

	public string Name { get; }
	public string Description { get; }
}