using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meepo.Core.Configs;

namespace Meepo.Core.Client
{
    internal interface IClientManager
    {
        void Listen();

        Task SendToClientAsync(Guid id, byte[] bytes);

        Task SendToClientsAsync(byte[] bytes);

        Dictionary<Guid, TcpAddress> GetServerClientInfos();

        void RemoveClient(Guid id);
    }
}