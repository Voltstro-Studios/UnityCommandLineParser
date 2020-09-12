using System;

namespace Voltstro.CommandLineParser
{
	[AttributeUsage(AttributeTargets.Field)]
	public class CommandLineArgumentAttribute : Attribute
	{
		public CommandLineArgumentAttribute(string name)
		{
			Name = name;
		}

		public readonly string Name;
	}
}