using System;
using System.IO;
using McMaster.Extensions.CommandLineUtils;

#nullable enable
namespace UnityCommandLineParser
{
    public class Console : IConsole
    {
        public void ResetColor()
        {
            
        }

        public TextWriter Out { get; } = TextWriter.Null;
        
        public TextWriter Error { get; } = TextWriter.Null;
        
        public TextReader In { get; } = TextReader.Null;

        public bool IsInputRedirected { get; } = false;
        
        public bool IsOutputRedirected { get; } = false;
        
        public bool IsErrorRedirected { get; } = false;
        
        public ConsoleColor ForegroundColor { get; set; }
        
        public ConsoleColor BackgroundColor { get; set; }
        
        public event ConsoleCancelEventHandler? CancelKeyPress = (sender, args) =>
        {
        };
    }
}