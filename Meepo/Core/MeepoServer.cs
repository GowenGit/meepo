using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core.Client;
using Meepo.Core.Configs;
using Meepo.Core.Exceptions;
using Meepo.Core.Logging;

namespace Meepo.Core
{
    internal class MeepoServer : IMeepoServer
    {
        private readonly IClientManagerProvider clientManagerProvider;
        private readonly ILogger logger;

        private IClientManager clientManager;

        public MeepoServer(IClientManagerProvider clientManagerProvider, MeepoConfig config)
        {
            this.clientManagerProvider = clientManagerProvider;

            logger = config.Logger;
        }

        public void StartServer(CancellationToken cancellationToken)
        {
            try
            {
                clientManager = clientManagerProvider.GetClientManager(cancellationToken);
            }
            catch (MeepoException)
            {
                return;
            }

            logger.Message("Server is running...");

            try
            {
                Task.Factory.StartNew(() => clientManager.Listen(), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.Message("Server has stopped");
            }
            catch (Exception ex)
            {
                logger.Error("Unknown server error", ex);
            }
        }

        public async Task SendToClientAsync(Guid id, byte[] bytes)
        {
            await clientManager.SendToClientAsync(id, bytes);
        }

        public async Task SendToClientsAsync(byte[] bytes)
        {
            await clientManager.SendToClientsAsync(bytes);
        }

        public void RemoveClient(Guid id)
        {
            clientManager.RemoveClient(id);
        }

        public Dictionary<Guid, TcpAddress> GetServerClientInfos()
        {
            return clientManager.GetServerClientInfos();
        }
    }
}
