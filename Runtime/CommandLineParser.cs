using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class CommandLineParser
{
	private static Dictionary<Type, ITypeReader> typeReaders = new Dictionary<Type, ITypeReader>
	{
		[typeof(string)] = new StringReader()
	};

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Init()
	{
		Init(Environment.GetCommandLineArgs());
	}

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
				if (typeReaders.TryGetValue(property.FieldType, out ITypeReader reader))
				{
					property.SetValue(property, reader.ReadType(value));
				}
			}
			i++;
		}
	}
}