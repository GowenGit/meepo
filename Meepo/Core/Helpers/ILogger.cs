using System;

namespace Meepo.Core.Helpers
{
    public interface ILogger
    {
        void Error(string message, Exception ex);

        void Message(string message);

        void Warning(string message);
    }
}