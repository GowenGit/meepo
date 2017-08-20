using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core.Configs;

namespace Meepo.Core
{
    internal interface IMeepoServer
    {
        Dictionary<Guid, TcpAddress> GetServerClientInfos();

        void StartServer(CancellationToken cancellationToken);

        Task SendToClientAsync(Guid id, byte[] bytes);

        Task SendToClientsAsync(byte[] bytes);

        void RemoveClient(Guid id);
    }
}