using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meepo.Core.Configs;

namespace Meepo.Core.Client
{
    internal interface IClientManager
    {
        void Listen();

        Task SendToClient(Guid id, byte[] bytes);

        Task SendToClients(byte[] bytes);

        Dictionary<Guid, TcpAddress> GetServerClientInfos();
    }
}