using System;
using Meepo.Core.Exceptions;

namespace Meepo.Core.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Message(string message)
        {
            Console.WriteLine(message);
        }

        public void Warning(string message)
        {
            Console.WriteLine($"Warning: {message}");
        }

        public void Error(string message, Exception ex)
        {
            Console.WriteLine($"Message: {message}{Environment.NewLine}" +
                              $"Error: {ex.ToFormattedException()}");
        }
    }
}
