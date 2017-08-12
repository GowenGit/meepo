using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core.Configs;
using Meepo.Core.Helpers;

namespace Meepo.Core
{
    internal class MeepoServer : IMeepoServer
    {
        private readonly TcpAddress machineAddress;
        private readonly IEnumerable<TcpAddress> serverAddresses;
        private readonly ILogger logger;
        private readonly MessageReceivedHandler messageReceived;

        private IClientManager clientManager;

        public MeepoServer(
            TcpAddress machineAddress,
            IEnumerable<TcpAddress> serverAddresses,
            ILogger logger,
            MessageReceivedHandler messageReceived)
        {
            this.machineAddress = machineAddress;
            this.serverAddresses = serverAddresses;
            this.logger = logger;
            this.messageReceived = messageReceived;
        }

        public async Task RunServer(CancellationToken cancellationToken)
        {
            var listener = new TcpListener(machineAddress.IPAddress, machineAddress.Port);

            try
            {
                listener.Start();
                logger.Message($"Server at {machineAddress.IPAddress}:{machineAddress.Port} has started...");
            }
            catch (Exception ex)
            {
                logger.Error("Server start failed", ex);
                return;
            }

            clientManager = new ClientManager(listener, serverAddresses, cancellationToken, logger, messageReceived);

            try
            {
                await Task.Factory.StartNew(() => clientManager.Listen(), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.Message("Server has stopped.");
            }
        }

        public async Task SendToClient(Guid id, byte[] bytes)
        {
            await clientManager.SendToClient(id, bytes);
        }

        public async Task SendToClients(byte[] bytes)
        {
            await clientManager.SendToClients(bytes);
        }

        public Dictionary<Guid, TcpAddress> GetServerClientInfos()
        {
            return clientManager.GetServerClientInfos();
        }
    }
}
