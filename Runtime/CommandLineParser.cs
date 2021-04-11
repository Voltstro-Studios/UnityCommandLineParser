using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using UnityCommandLineParser.TypeReader;
using UnityEngine;

namespace UnityCommandLineParser
{
	/// <summary>
	///     The main class for parsing command line arguments
	/// </summary>
	public static class CommandLineParser
	{
		private static readonly Dictionary<Type, ITypeReader> TypeReaders = new Dictionary<Type, ITypeReader>
		{
			[typeof(string)] = new StringReader(),
			[typeof(int)] = new IntReader(),
			[typeof(byte)] = new ByteReader(),
			[typeof(float)] = new FloatReader(),
			[typeof(bool)] = new BoolReader()
		};

		/// <summary>
		///     Adds a new, or overrides a TypeReader used for knowing what to set when parsing the arguments
		/// </summary>
		/// <param name="type"></param>
		/// <param name="reader"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void AddTypeReader([NotNull] Type type, [NotNull] ITypeReader reader)
		{
			//Make sure our arguments are not null
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			//If the type reader already exists, we override the old one with the one we are adding
			if (TypeReaders.ContainsKey(type))
			{
				TypeReaders[type] = reader;
				return;
			}

			TypeReaders.Add(type, reader);
		}
		
		#region Initialization
		
		/// <summary>
		///     Initializes and parses the command line arguments
		///     <para>
		///         This function is automatically called on Subsystem Registration using Unity's
		///         <see cref="RuntimeInitializeOnLoadMethodAttribute" />
		///     </para>
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		public static void Init()
		{
			Init(Environment.GetCommandLineArgs());
		}
		
		/// <summary>
		///     Initializes and parses the command line arguments
		/// </summary>
		/// <param name="args"></param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		[PublicAPI]
		public static void Init([NotNull] string[] args)
		{
			//Make sure args are not null
			if (args == null)
				throw new ArgumentNullException(nameof(args));

			CommandLineApplication commandLineApp = new CommandLineApplication
			{
				UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue
			};

			//Add all of our commands to commandLineApp
			Dictionary<CommandOption, FieldInfo> arguments = new Dictionary<CommandOption, FieldInfo>();
			foreach (KeyValuePair<FieldInfo,CommandLineArgumentAttribute> argument in GetCommandFields())
			{
				CommandOption option = commandLineApp.Option($"-{argument.Value.Name} <{argument.Value.Name.ToUpper()}>", argument.Value.Description, CommandOptionType.SingleValue);
				arguments.Add(option, argument.Key);
			}
			
			commandLineApp.OnExecute(() =>
			{
				foreach (KeyValuePair<CommandOption,FieldInfo> argument in arguments)
				{
					if(!argument.Key.HasValue())
						continue;
					
					if (TypeReaders.TryGetValue(argument.Value.FieldType, out ITypeReader reader))
						argument.Value.SetValue(argument.Value, reader.ReadType(argument.Key.Value()));

					//Handling for enums
					if (argument.Value.FieldType.IsEnum)
					{
						Type baseType = Enum.GetUnderlyingType(argument.Value.FieldType);
						if(!TypeReaders.TryGetValue(baseType, out reader))
							continue;

						object enumValue = Enum.ToObject(argument.Value.FieldType, reader.ReadType(argument.Key.Value()));
						argument.Value.SetValue(argument.Value, enumValue);
					}
				}
			});

			//Parse our commands
			commandLineApp.Execute(args);
		}

		/// <summary>
		///     Gets all fields with the <see cref="CommandLineArgumentAttribute" /> attached
		/// </summary>
		/// <returns></returns>
		public static Dictionary<FieldInfo, CommandLineArgumentAttribute> GetCommandFields()
		{
			const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			IEnumerable<FieldInfo> fields = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => x.GetTypes())
				.SelectMany(x => x.GetFields(bindingFlags))
				.Where(x => x.GetCustomAttribute<CommandLineArgumentAttribute>() != null);
			return fields.ToDictionary(fieldInfo => fieldInfo,
				fieldInfo => fieldInfo.GetCustomAttribute<CommandLineArgumentAttribute>());
		}

		#endregion
	}
}