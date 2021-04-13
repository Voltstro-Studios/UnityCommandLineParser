using System;
using JetBrains.Annotations;

namespace UnityCommandLineParser
{
    /// <summary>
    ///     An <see cref="Attribute" /> for making a static method to be run when a certain argument is included
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [PublicAPI]
    public class CommandLineCommandAttribute : Attribute
    {
        /// <summary>
        ///     Marks a method to be called if the argument is provided
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandLineCommandAttribute([NotNull] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Description = "";
        }

        /// <summary>
        ///     Marks a method to be called if the argument is provided
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CommandLineCommandAttribute([NotNull] string name, [NotNull] string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));

            Name = name;
            Description = description;
        }

        /// <summary>
        ///     What argument is this using
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The description of this argument
        /// </summary>
        public string Description { get; }
    }
}