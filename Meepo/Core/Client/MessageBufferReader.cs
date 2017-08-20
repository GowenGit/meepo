using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Meepo.Core.Client
{
    internal class MessageBufferReader
    {
        private readonly TcpClient client;

        private readonly MessageReceivedHandler messageReceived;

        private bool awaitingMessage;
        private int awaitingMessageSize;

        public MessageBufferReader(TcpClient client, MessageReceivedHandler messageReceived)
        {
            this.client = client;
            this.messageReceived = messageReceived;
        }

        public async Task Read(NetworkStream stream, CancellationToken cancellationToken, Guid id)
        {
            if (!awaitingMessage)
            {
                if (client.Available >= 4)
                {
                    awaitingMessage = true;

                    var bytes = new byte[4];

                    await stream.ReadAsync(bytes, 0, 4, cancellationToken);

                    awaitingMessageSize = BitConverter.ToInt32(bytes, 0);
                }
            }

            if (awaitingMessage)
            {
                if (client.Available >= awaitingMessageSize)
                {
                    awaitingMessage = true;

                    var bytes = new byte[awaitingMessageSize];

                    await stream.ReadAsync(bytes, 0, awaitingMessageSize, cancellationToken);

                    awaitingMessage = false;

                    var args = new MessageReceivedEventArgs(id, bytes);

                    messageReceived?.Invoke(args);
                }
            }
        }
    }
}
