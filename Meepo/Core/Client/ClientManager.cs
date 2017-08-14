using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core.Configs;
using Meepo.Core.Helpers;
using Meepo.Util;

namespace Meepo.Core.Client
{
    internal class ClientManager : IClientManager
    {
        private readonly TcpListener listener;
        private readonly IEnumerable<TcpAddress> serverAddresses;

        private readonly CancellationToken cancellationToken;

        private readonly ClientFactory clientFactory;
        private readonly ConcurrentSet<ClientWrapper> allClients = new ConcurrentSet<ClientWrapper>();

        public ClientManager(
            TcpListener listener,
            IEnumerable<TcpAddress> serverAddresses,
            CancellationToken cancellationToken,
            ILogger logger,
            MessageReceivedHandler messageReceived)
        {
            this.listener = listener;
            this.serverAddresses = serverAddresses;
            this.cancellationToken = cancellationToken;

            clientFactory = new ClientFactory(logger, cancellationToken, messageReceived, RemoveClient);
        }

        public void Listen()
        {
            ConnectToServers();

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    RemoveClients();
                    break;
                }

                var listenerThread = listener.AcceptTcpClientAsync();

                listenerThread.Wait(cancellationToken);

                var client = listenerThread.Result;

                var clientWrapper = clientFactory.GetClient(client);

                allClients.Add(clientWrapper);
            }
        }

        private void ConnectToServers()
        {
            foreach (var address in serverAddresses)
            {
                Task.Factory.StartNew(() =>
                {
                    var client = clientFactory.GetClient(address);

                    allClients.Add(client);
                }, cancellationToken);
            }
        }

        public async Task SendToClient(Guid id, byte[] bytes)
        {
            var client = allClients[id];
            await client.Send(bytes);
        }

        public async Task SendToClients(byte[] bytes)
        {
            foreach (var clientWrapper in allClients)
            {
                await clientWrapper.Send(bytes);
            }
        }

        public Dictionary<Guid, TcpAddress> GetServerClientInfos()
        {
            var result = allClients
                .Where(x => x.IsToServer)
                .ToDictionary(x => x.Id, x => x.Address);

            return result;
        }

        private void RemoveClients()
        {
            foreach (var clientWrapper in allClients)
            {
                clientWrapper.Close();
                RemoveClient(clientWrapper.Id);
            }
        }

        private void RemoveClient(Guid id)
        {
            allClients.Remove(id);
        }
    }
}
