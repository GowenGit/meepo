using System;

namespace Meepo.Core.Helpers
{
    public delegate void MessageReceivedHandler(MessageReceivedEventArgs e);

    public delegate void ClientConnectionFailed(Guid clientId);
    
    public class MessageReceivedEventArgs : EventArgs
    {
        public Guid Id { get; }

        public byte[] Bytes { get; }

        public MessageReceivedEventArgs(Guid id, byte[] messageBytes)
        {
            Id = id;
            Bytes = messageBytes;
        }
    }
}
