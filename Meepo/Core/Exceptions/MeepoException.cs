using System;

namespace Meepo.Core.Exceptions
{
    public class MeepoException : Exception
    {
        public MeepoException(string message) : base(message) { }

        public MeepoException(string message, Exception ex) : base(message, ex) { }
    }
}
