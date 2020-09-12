using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Voltstro.CommandLineParser.TypeReaders;

namespace Voltstro.CommandLineParser
{
	/// <summary>
	/// The main class for parsing the command line arguments
	/// </summary>
	public static class CommandLineParser
	{
		private static readonly Dictionary<Type, ITypeReader> TypeReaders = new Dictionary<Type, ITypeReader>
		{
			[typeof(string)] = new StringReader(),
			[typeof(int)] = new IntReader(),
			[typeof(float)] = new FloatReader(),
			[typeof(bool)] = new BoolReader()
		};

		/// <summary>
		/// Adds a new, or overrides a TypeReader used for knowing what to set when parsing the arguments
		/// </summary>
		/// <param name="type"></param>
		/// <param name="reader"></param>
		public static void AddTypeReader(Type type, ITypeReader reader)
		{
			if (TypeReaders.ContainsKey(type))
			{
				TypeReaders[type] = reader;
				return;
			}

			TypeReaders.Add(type, reader);
		}

		#region Initialization

		/// <summary>
		/// Initializes and parses the command line arguments
		/// <para>This function is automatically called on Subsystem Registration using Unity's <see cref="RuntimeInitializeOnLoadMethodAttribute"/></para>
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		public static void Init()
		{
			Init(Environment.GetCommandLineArgs());
		}

		/// <summary>
		/// Initializes and parses the command line arguments
		/// </summary>
		/// <param name="args"></param>
		public static void Init(string[] args)
		{
			Dictionary<string, FieldInfo> argumentProperties = new Dictionary<string, FieldInfo>();

			//Find any properties with the CommandLineArgument attribute
			const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			IEnumerable<FieldInfo> props = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => x.GetTypes())
				.SelectMany(x => x.GetFields(bindingFlags))
				.Where(x => x.GetCustomAttribute<CommandLineArgumentAttribute>() != null);

			//Go through all found properties and add them to argumentProperties
			foreach (FieldInfo propertyInfo in props)
			{
				CommandLineArgumentAttribute attribute = propertyInfo.GetCustomAttribute<CommandLineArgumentAttribute>();
				if (argumentProperties.ContainsKey(attribute.Name))
					throw new Exception($"The argument {attribute.Name} has already been defined!");

				argumentProperties.Add(attribute.Name, propertyInfo);
			}

			//Now sort through all the arguments and set the corresponding argument
			int i = 0;
			while (i < args.Length)
			{
				string arg = args[i];
				if (!arg.StartsWith("-"))
				{
					i++;
					continue;
				}

				string value = null;
				if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
				{
					value = args[i + 1];
					i++;
				}

				if (argumentProperties.TryGetValue(arg.Replace("-", ""), out FieldInfo property))
				{
					//Handle reading and setting the type
					if (TypeReaders.TryGetValue(property.FieldType, out ITypeReader reader))
					{
						property.SetValue(property, reader.ReadType(value));
					}
				}
				i++;
			}
		}

		#endregion
	}
}