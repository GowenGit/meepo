using System;

namespace Meepo.Core.Helpers
{
    internal class Logger : ILogger
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
