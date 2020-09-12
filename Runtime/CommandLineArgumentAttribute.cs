using System;

[AttributeUsage(AttributeTargets.Property)]
public class CommandLineArgumentAttribute : Attribute
{
	public CommandLineArgumentAttribute(string name)
	{
		Name = name;
	}

	public readonly string Name;
}