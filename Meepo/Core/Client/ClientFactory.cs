using System.Net.Sockets;
using System.Threading;
using Meepo.Core.Configs;
using Meepo.Core.Helpers;

namespace Meepo.Core.Client
{
    internal class ClientFactory
    {
        private readonly ILogger logger;
        private readonly CancellationToken cancellationToken;
        private readonly MessageReceivedHandler messageReceived;
        private readonly ClientConnectionFailed clientConnectionFailed;

        public ClientFactory(ILogger logger,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            ClientConnectionFailed clientConnectionFailed)
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
