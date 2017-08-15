using System;

namespace Meepo.Serialization.Core.Exceptions
{
    internal class MeepoSerializationException : Exception
    {
        public MeepoSerializationException(string message) : base(message) { }
    }
}
