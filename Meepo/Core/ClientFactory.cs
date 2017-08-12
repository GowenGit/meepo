using System;
using System.Net.Sockets;
using System.Threading;
using Meepo.Core.Configs;
using Meepo.Core.Helpers;

namespace Meepo.Core
{
    internal class ClientFactory : IClientFactory
    {
        private readonly ILogger logger;
        private readonly CancellationToken cancellationToken;
        private readonly MessageReceivedHandler messageReceived;
        private readonly Action<Guid> clientConnectionFailed;

        public ClientFactory(ILogger logger,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            Action<Guid> clientConnectionFailed)
        {
            this.logger = logger;
            this.cancellationToken = cancellationToken;
            this.messageReceived = messageReceived;
            this.clientConnectionFailed = clientConnectionFailed;
        }

        public ClientWrapper GetClient(TcpClient client)
        {
            return new ClientWrapper(
                client,
                logger,
                cancellationToken,
                messageReceived,
                clientConnectionFailed);
        }

        public ClientWrapper GetClient(TcpAddress address)
        {
            return new ClientWrapper(
                address,
                logger,
                cancellationToken,
                messageReceived,
                clientConnectionFailed);
        }
    }
}
