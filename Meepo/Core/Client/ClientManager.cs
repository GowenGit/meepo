using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core.Configs;
using Meepo.Core.Exceptions;
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
            MeepoConfig config,
            MessageReceivedHandler messageReceived)
        {
            this.listener = listener;
            this.serverAddresses = serverAddresses;
            this.cancellationToken = cancellationToken;

            clientFactory = new ClientFactory(config, cancellationToken, messageReceived, RemoveClient);
        }

        public async void Listen()
        {
            ConnectToServers();

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    RemoveClients();
                    break;
                }

                var client = await listener.AcceptTcpClientAsync();

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

        public async Task SendToClientAsync(Guid id, byte[] bytes)
        {
            var client = allClients[id];

            if (client == null) throw new MeepoException($"No client with ID {id} was found!");

            await client.Send(bytes);
        }

        public async Task SendToClientsAsync(byte[] bytes)
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
                clientWrapper.Dispose();
                RemoveClient(clientWrapper.Id);
            }
        }

        public void RemoveClient(Guid id)
        {
            allClients.Remove(id);
        }
    }
}
