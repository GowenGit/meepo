using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core.Client;
using Meepo.Core.Configs;
using Meepo.Core.Helpers;

namespace Meepo.Core
{
    internal class MeepoServer : IMeepoServer
    {
        private readonly IClientManagerProvider clientManagerProvider;
        private readonly ILogger logger;

        private IClientManager clientManager;

        public MeepoServer(IClientManagerProvider clientManagerProvider, ILogger logger)
        {
            this.clientManagerProvider = clientManagerProvider;
            this.logger = logger;
        }

        public async Task StartServer(CancellationToken cancellationToken)
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
                await Task.Factory.StartNew(() => clientManager.Listen(), cancellationToken);
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
