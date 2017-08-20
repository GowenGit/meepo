﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core;
using Meepo.Core.Configs;
using Meepo.Core.StateMachine;

namespace Meepo
{
    public class Meepo : IMeepo
    {
        private readonly MeepoStateMachine stateMachine;

        private readonly IMeepoServer server;

        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Server state.
        /// </summary>
        public State ServerState => stateMachine.CurrenState;

        /// <summary>
        /// This event is fired whenever server 
        /// receives a message.
        /// </summary>
        public event MessageReceivedHandler MessageReceived;

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        public Meepo(TcpAddress listenerAddress) : this(listenerAddress, new TcpAddress[] { }, new MeepoConfig()) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        /// <param name="config">Custom Meepo configuration</param>
        public Meepo(TcpAddress listenerAddress, MeepoConfig config) : this(listenerAddress, new TcpAddress[] { }, config) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        /// <param name="serverAddresses">List of server addresses to connect to</param>
        public Meepo(TcpAddress listenerAddress, IEnumerable<TcpAddress> serverAddresses) : this(listenerAddress, serverAddresses, new MeepoConfig()) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listenerAddress">Address you want to expose</param>
        /// <param name="serverAddresses">List of server addresses to connect to</param>
        /// <param name="config">Custom Meepo configuration</param>
        public Meepo(TcpAddress listenerAddress, IEnumerable<TcpAddress> serverAddresses, MeepoConfig config)
        {
            var clientManagerProvider = new ClientManagerProvider(config, listenerAddress, serverAddresses, OnMessageReceived);
            server = new MeepoServer(clientManagerProvider, config);
            stateMachine = new MeepoStateMachine(config.Logger);
        } 
        #endregion

        /// <summary>
        /// Run Meepo server.
        /// Starts listening for new clients
        /// and connects to specified servers.
        /// </summary>
        public void Start()
        {
            if (stateMachine.MoveNext(Command.Start) == State.Invalid) return;

            cancellationTokenSource = new CancellationTokenSource();

            server.StartServer(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Stop Meepo server.
        /// </summary>
        public void Stop()
        {
            if (stateMachine.MoveNext(Command.Stop) == State.Invalid) return;

            cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Remove client.
        /// </summary>
        /// <param name="id">Client ID</param>
        public void RemoveClient(Guid id)
        {
            if (stateMachine.MoveNext(Command.RemovieClient) == State.Invalid) return;

            server.RemoveClient(id);
        }

        /// <summary>
        /// Send message to a specific client.
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="bytes">Bytes to send</param>
        /// <returns></returns>
        public async Task SendAsync(Guid id, byte[] bytes)
        {
            if (stateMachine.MoveNext(Command.SendToClient) == State.Invalid) return;

            await server.SendToClientAsync(id, bytes);
        }

        /// <summary>
        /// Send message to all clients.
        /// Including connected servers.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task SendAsync(byte[] bytes)
        {
            if (stateMachine.MoveNext(Command.SendToClients) == State.Invalid) return;

            await server.SendToClientsAsync(bytes);
        }

        /// <summary>
        /// Get IDs and addresses of all connected servers.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, TcpAddress> GetServerClientInfos()
        {
            return stateMachine.MoveNext(Command.GetClientIds) == State.Invalid ? new Dictionary<Guid, TcpAddress>() : server.GetServerClientInfos();
        }

        private void OnMessageReceived(MessageReceivedEventArgs args)
        {
            MessageReceived?.Invoke(args);
        }

        /// <summary>
        /// Stop and dispose Meepo server.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
