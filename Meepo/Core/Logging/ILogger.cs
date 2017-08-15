using System;

namespace Meepo.Core.Logging
{
    public interface ILogger
    {
        void Error(string message, Exception ex);

        void Message(string message);

        void Warning(string message);
    }
}