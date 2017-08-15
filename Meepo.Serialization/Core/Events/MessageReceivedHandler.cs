using System;

namespace Meepo.Serialization.Core.Events
{
    public delegate void MessageReceivedHandler<in T>(Guid id, T data);
}
