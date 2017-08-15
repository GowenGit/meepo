using System;

// ReSharper disable once CheckNamespace
public delegate void MessageReceivedHandler(MessageReceivedEventArgs e);

public delegate void ClientConnectionFailed(Guid clientId);
    
public class MessageReceivedEventArgs : EventArgs
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Guid Id { get; }

    public byte[] Bytes { get; }

    public MessageReceivedEventArgs(Guid id, byte[] messageBytes)
    {
        Id = id;
        Bytes = messageBytes;
    }
}

