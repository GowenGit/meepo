using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core.Configs;
using Meepo.Core.Exceptions;

namespace Meepo.Core.Client
{
    internal class MessageBufferReader
    {
        private readonly MeepoConfig config;
        private readonly TcpClient client;

        private readonly MessageReceivedHandler messageReceived;

        private bool awaitingMessage;
        private int awaitingMessageSize;

        public MessageBufferReader(MeepoConfig config, TcpClient client, MessageReceivedHandler messageReceived)
        {
            this.config = config;
            this.client = client;
            this.messageReceived = messageReceived;
        }

        public async Task Read(NetworkStream stream, CancellationToken cancellationToken, Guid id)
        {
            if (!awaitingMessage)
            {
                if (client.Available >= 4)
                {
                    var bytes = new byte[4];

                    await stream.ReadAsync(bytes, 0, 4, cancellationToken);

                    awaitingMessageSize = BitConverter.ToInt32(bytes, 0);

                    if (awaitingMessageSize > config.BufferSizeInBytes)
                    {
                        throw new MeepoException($"Buffer size {config.BufferSizeInBytes} (bytes) is less than the incoming message size {awaitingMessageSize} (bytes)!");
                    }

                    awaitingMessage = true;
                }
            }

            if (awaitingMessage)
            {
                if (client.Available >= awaitingMessageSize)
                {
                    var bytes = new byte[awaitingMessageSize];

                    if (awaitingMessageSize > 0)
                    {
                        await stream.ReadAsync(bytes, 0, awaitingMessageSize, cancellationToken);
                    }      

                    awaitingMessage = false;

                    var args = new MessageReceivedEventArgs(id, bytes);

                    messageReceived?.Invoke(args);
                }
            }
        }
    }
}
