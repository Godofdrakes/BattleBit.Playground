using System;

namespace Playground.Common;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
	public string Name { get; }
	public string Description { get; }
	public string? RequiredPermission { get; }

	public CommandAttribute(string name, string? description = default, string? requiredPermission = default)
	{
		Name = name;
		Description = description ?? string.Empty;
		RequiredPermission = requiredPermission;
	}
}