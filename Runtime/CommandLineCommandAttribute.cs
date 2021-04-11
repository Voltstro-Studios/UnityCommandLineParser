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
        public string Name { get; }
        public string Description { get; }

        public CommandLineCommandAttribute([NotNull] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            Name = name;
            Description = "";
        }
        
        public CommandLineCommandAttribute([NotNull] string name, [NotNull] string description)
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