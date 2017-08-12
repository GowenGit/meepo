using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meepo.Core.Configs;
using Meepo.Core.Helpers;
using Meepo.Core.StateMachine;

namespace Meepo
{
    public interface IMeepo
    {
        State ServerState { get; }

        event MessageReceivedHandler MessageReceived;

        Dictionary<Guid, TcpAddress> GetServerClientInfos();

        void RunServer();

        Task SendToClient(Guid id, byte[] bytes);

        Task SendToClients(byte[] bytes);

        void StopServer();
    }
}