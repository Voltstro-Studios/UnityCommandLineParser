using System;
using JetBrains.Annotations;

namespace Voltstro.CommandLineParser
{
	/// <summary>
	///     An <see cref="Attribute" /> for making a static field be set when the supplied arguments is provided
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	[PublicAPI]
	public class CommandLineArgumentAttribute : Attribute
	{
		/// <summary>
		///     What argument this is using
		/// </summary>
		public string Name { get; }
		
		/// <summary>
		///		The description of the argument
		/// </summary>
		public string Description { get;  }

		/// <summary>
		///     Set a field to be set by the command line parser if the argument is supplied
		/// </summary>
		/// <param name="name"></param>
		public CommandLineArgumentAttribute([NotNull] string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			Name = name;
			Description = "";
		}

		/// <summary>
		///     Set a field to be set by the command line parser if the argument is supplied
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		public CommandLineArgumentAttribute([NotNull] string name, [NotNull] string description)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			if (string.IsNullOrWhiteSpace(description))
				throw new ArgumentNullException(nameof(description));

			Name = name;
			Description = description;
		}
	}
}