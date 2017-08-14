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
        public State ServerState => stateMachine.CurrenState;
        private readonly MeepoStateMachine stateMachine;

        private readonly IMeepoServer server;   

        private CancellationTokenSource cancellationTokenSource;

        public event MessageReceivedHandler MessageReceived;

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        public Meepo(TcpAddress listenerAddress) : this(listenerAddress, new TcpAddress[] { }, new Logger()) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        /// <param name="logger">Custom ILogger implementation</param>
        public Meepo(TcpAddress listenerAddress, ILogger logger) : this(listenerAddress, new TcpAddress[] { }, logger) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        /// <param name="serverAddresses">List of server addresses to connect to</param>
        public Meepo(TcpAddress listenerAddress, IEnumerable<TcpAddress> serverAddresses) : this(listenerAddress, serverAddresses, new Logger()) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        /// <param name="serverAddresses">List of server addresses to connect to</param>
        /// <param name="logger">Custom ILogger implementation</param>
        public Meepo(TcpAddress listenerAddress, IEnumerable<TcpAddress> serverAddresses, ILogger logger)
        {
            var clientManagerProvider = new ClientManagerProvider(logger, listenerAddress, serverAddresses, OnMessageReceived);
            server = new MeepoServer(clientManagerProvider, logger);
            stateMachine = new MeepoStateMachine(logger);
        } 
        #endregion

        /// <summary>
        /// Run meepo server.
        /// Starts listening for new clients
        /// and connects to specified servers.
        /// </summary>
        public async void Start()
        {
            if (stateMachine.MoveNext(Command.Start) == State.Invalid) return;

            cancellationTokenSource = new CancellationTokenSource();

            await server.StartServer(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Stop meepo server.
        /// </summary>
        public void Stop()
        {
            if (stateMachine.MoveNext(Command.Stop) == State.Invalid) return;

            cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Send message to a specific client.
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="bytes">Bytes to send</param>
        /// <returns></returns>
        public async Task Send(Guid id, byte[] bytes)
        {
            if (stateMachine.MoveNext(Command.SendToClient) == State.Invalid) return;

            await server.SendToClient(id, bytes);
        }

        /// <summary>
        /// Send message to all clients.
        /// Including connected servers.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task Send(byte[] bytes)
        {
            if (stateMachine.MoveNext(Command.SendToClients) == State.Invalid) return;

            await server.SendToClients(bytes);
        }

        /// <summary>
        /// Get IDs and addresses of all connected servers.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, TcpAddress> GetServerClientInfos()
        {
            if (stateMachine.MoveNext(Command.GetClientIds) == State.Invalid) return new Dictionary<Guid, TcpAddress>();

            return server.GetServerClientInfos();
        }

        private void OnMessageReceived(MessageReceivedEventArgs args)
        {
            MessageReceived?.Invoke(args);
        }
    }
}
