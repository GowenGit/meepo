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

        Task RunServer(CancellationToken cancellationToken);

        Task SendToClient(Guid id, byte[] bytes);

        Task SendToClients(byte[] bytes);
    }
}