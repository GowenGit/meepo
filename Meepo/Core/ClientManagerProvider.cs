using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Meepo.Core.Client;
using Meepo.Core.Configs;
using Meepo.Core.Exceptions;
using Meepo.Core.Logging;

namespace Meepo.Core
{
    internal class ClientManagerProvider : IClientManagerProvider
    {
        private readonly ILogger logger;
        private readonly TcpAddress listenerAddress;
        private readonly IEnumerable<TcpAddress> serverAddresses;
        private readonly MessageReceivedHandler messageReceived;

        public ClientManagerProvider(
            ILogger logger,
            TcpAddress listenerAddress,
            IEnumerable<TcpAddress> serverAddresses,
            MessageReceivedHandler messageReceived)
        {
            this.logger = logger;
            this.listenerAddress = listenerAddress;
            this.serverAddresses = serverAddresses;
            this.messageReceived = messageReceived;
        }

        public IClientManager GetClientManager(CancellationToken cancellationToken)
        {
            TcpListener listener;

            try
            {
                listener = new TcpListener(listenerAddress.IPAddress, listenerAddress.Port);

                listener.Start();
            }
            catch (Exception ex)
            {
                const string message = "Failed to start listener";

                logger.Error(message, ex);

                throw new MeepoException(message, ex);
            }

            logger.Message($"Listener at {listenerAddress.IPAddress}:{listenerAddress.Port} has started...");

            return new ClientManager(listener, serverAddresses, cancellationToken, logger, messageReceived);
        }
    }
}
