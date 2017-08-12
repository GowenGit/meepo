using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core;
using Meepo.Core.Configs;
using Meepo.Core.Helpers;
using Meepo.Core.StateMachine;

namespace Meepo
{
    public class Meepo : IMeepo
    {
        public event MessageReceivedHandler MessageReceived;

        public State ServerState => stateMachine.CurrenState;

        private readonly MeepoStateMachine stateMachine;
        private readonly IMeepoServer server;

        private CancellationTokenSource cancellationTokenSource;

        #region Constructors
        public Meepo(TcpAddress machineAddress) : this(machineAddress, new TcpAddress[] { }, new Logger())
        {

        }

        public Meepo(TcpAddress machineAddress, IEnumerable<TcpAddress> serverAddresses) : this(machineAddress, serverAddresses, new Logger())
        {

        }

        public Meepo(TcpAddress machineAddress, ILogger logger) : this(machineAddress, new TcpAddress[] { }, logger)
        {

        }

        public Meepo(TcpAddress machineAddress, IEnumerable<TcpAddress> serverAddresses, ILogger logger)
        {
            server = new MeepoServer(machineAddress, serverAddresses, logger, OnMessageReceived);
            stateMachine = new MeepoStateMachine(logger);
        } 
        #endregion

        public async void RunServer()
        {
            if (stateMachine.MoveNext(Command.Start) == State.Invalid) return;

            cancellationTokenSource = new CancellationTokenSource();

            await server.RunServer(cancellationTokenSource.Token);
        }

        public void StopServer()
        {
            if (stateMachine.MoveNext(Command.Stop) == State.Invalid) return;

            cancellationTokenSource.Cancel();
        }

        public async Task SendToClient(Guid id, byte[] bytes)
        {
            if (stateMachine.MoveNext(Command.SendToClient) == State.Invalid) return;

            await server.SendToClient(id, bytes);
        }

        public async Task SendToClients(byte[] bytes)
        {
            if (stateMachine.MoveNext(Command.SendToClients) == State.Invalid) return;

            await server.SendToClients(bytes);
        }

        public Dictionary<Guid, TcpAddress> GetServerClientInfos()
        {
            if (stateMachine.MoveNext(Command.GetClientIds) == State.Invalid) return new Dictionary<Guid, TcpAddress>();

            return server.GetServerClientInfos();
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            MessageReceived?.Invoke(sender, args);
        }
    }
}
