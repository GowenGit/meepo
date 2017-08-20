using System;

namespace Meepo.Core.Logging
{
    public class SilentLogger : ILogger
    {
        public void Error(string message, Exception ex) { }

        public void Message(string message) { }

        public void Warning(string message) { }
    }
}
