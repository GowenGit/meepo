using System.Net.Sockets;
using System.Threading;
using Meepo.Core.Configs;

namespace Meepo.Core.Client
{
    internal class ClientFactory
    {
        private readonly MeepoConfig config;
        private readonly CancellationToken cancellationToken;
        private readonly MessageReceivedHandler messageReceived;
        private readonly ClientConnectionFailed clientConnectionFailed;

        public ClientFactory(MeepoConfig config,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            ClientConnectionFailed clientConnectionFailed)
        {
            this.config = config;
            this.cancellationToken = cancellationToken;
            this.messageReceived = messageReceived;
            this.clientConnectionFailed = clientConnectionFailed;
        }

        public ClientWrapper GetClient(TcpClient client)
        {
            return new ClientWrapper(
                client,
                config,
                cancellationToken,
                messageReceived,
                clientConnectionFailed);
        }

        public ClientWrapper GetClient(TcpAddress address)
        {
            return new ClientWrapper(
                address,
                config,
                cancellationToken,
                messageReceived,
                clientConnectionFailed);
        }
    }
}
