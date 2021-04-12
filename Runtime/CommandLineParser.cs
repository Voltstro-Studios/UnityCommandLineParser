using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using UnityEngine;

namespace UnityCommandLineParser
{
	/// <summary>
	///     The main class for parsing command line arguments
	/// </summary>
	public static class CommandLineParser
	{
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
#if !CLP_NO_AUTO_PARSE
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
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

			//Add all of our arguments to commandLineApp
			Dictionary<CommandOption, FieldInfo> arguments = new Dictionary<CommandOption, FieldInfo>();
			foreach (KeyValuePair<FieldInfo,CommandLineArgumentAttribute> argument in GetCommandLineArguments())
			{
				CommandOption option = commandLineApp.Option($"-{argument.Value.Name} <{argument.Value.Name.ToUpper()}>", argument.Value.Description, CommandOptionType.SingleValue);
				arguments.Add(option, argument.Key);
			}
			
			//Add all of our commands to commandLineApp
			Dictionary<CommandOption, Action> commands = new Dictionary<CommandOption, Action>();
			foreach (KeyValuePair<MethodInfo,CommandLineCommandAttribute> command in GetCommandLineCommands())
			{
				//Create command action
				Action action;
				try
				{
					//TODO: Implement ILogger
					action = (Action) Delegate.CreateDelegate(typeof(Action), command.Key);
				}
				catch (Exception)
				{
					continue;
				}
				
				CommandOption option = commandLineApp.Option($"-{command.Value.Name}", command.Value.Description,
					CommandOptionType.NoValue);
				commands.Add(option, action);
			}
			
			commandLineApp.OnExecute(() =>
			{
				//Parse all arguments
				foreach (KeyValuePair<CommandOption,FieldInfo> argument in arguments)
				{
					if(!argument.Key.HasValue())
						continue;
					
					IValueParser parser = commandLineApp.ValueParsers.GetParser(argument.Value.FieldType);
					
					//Epic fail ReSharper, because guess what, it can be null!
					// ReSharper disable once ConditionIsAlwaysTrueOrFalse
					if(parser == null)
						continue; //TODO: Log error with ILogger
					
					object parsedValue;
					try
					{
						parsedValue = parser.Parse("", argument.Key.Value(), CultureInfo.CurrentCulture);
					}
					catch (FormatException)
					{
						//TODO: Implement ILogger
						continue;
					}

					//Probs failed to parse
					if(parsedValue == null)
						continue;
					
					//Handling for enums
					if (argument.Value.FieldType.IsEnum)
					{
						object enumValue = Enum.ToObject(argument.Value.FieldType, parsedValue);
						argument.Value.SetValue(argument.Value, enumValue);
						continue;
					}
					
					argument.Value.SetValue(argument.Value, parsedValue);
				}
				
				//Parse all commands
				foreach (KeyValuePair<CommandOption,Action> command in commands)
				{
					if (command.Key.Values.Count <= 0) continue;
					try
					{
						command.Value.Invoke();
					}
					catch (Exception)
					{
						// ignored
					}
				}
			});

			//Parse our commands
			commandLineApp.Execute(args);
		}

		private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

		/// <summary>
		///     Gets all fields with the <see cref="CommandLineArgumentAttribute" /> attached
		/// </summary>
		/// <returns></returns>
		public static Dictionary<FieldInfo, CommandLineArgumentAttribute> GetCommandLineArguments()
		{
			IEnumerable<FieldInfo> fields = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(x => x.GetTypes())
				.SelectMany(x => x.GetFields(BindingFlags))
				.Where(x => x.GetCustomAttribute<CommandLineArgumentAttribute>() != null);
			return fields.ToDictionary(fieldInfo => fieldInfo,
				fieldInfo => fieldInfo.GetCustomAttribute<CommandLineArgumentAttribute>());
		}

		/// <summary>
		///		Gets all methods with the <see cref="CommandLineCommandAttribute"/> attached
		/// </summary>
		/// <returns></returns>
		public static Dictionary<MethodInfo, CommandLineCommandAttribute> GetCommandLineCommands()
		{
			IEnumerable<MethodInfo> methods = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
				.SelectMany(x => x.GetMethods(BindingFlags))
				.Where(x => x.GetCustomAttribute<CommandLineCommandAttribute>() != null);
			return methods.ToDictionary(method => method,
				method => method.GetCustomAttribute<CommandLineCommandAttribute>());
		}

		#endregion
	}
}