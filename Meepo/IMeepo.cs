using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meepo.Core.Configs;
using Meepo.Core.StateMachine;

namespace Meepo
{
    public interface IMeepo : IDisposable
    {
        /// <summary>
        /// Server state.
        /// </summary>
        State ServerState { get; }

        /// <summary>
        /// This event is fired whenever server 
        /// receives a message.
        /// </summary>
        event MessageReceivedHandler MessageReceived;

        /// <summary>
        /// Get IDs and addresses of all connected servers.
        /// </summary>
        /// <returns></returns>
        Dictionary<Guid, TcpAddress> GetServerClientInfos();

        /// <summary>
        /// Run Meepo server.
        /// Starts listening for new clients
        /// and connects to specified servers.
        /// </summary>
        void Start();

        /// <summary>
        /// Send message to a specific client.
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="bytes">Bytes to send</param>
        /// <returns></returns>
        Task SendAsync(Guid id, byte[] bytes);

        /// <summary>
        /// Send message to all clients.
        /// Including connected servers.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        Task SendAsync(byte[] bytes);

        /// <summary>
        /// Remove client.
        /// </summary>
        /// <param name="id">Client ID</param>
        void RemoveClient(Guid id);

        /// <summary>
        /// Stop Meepo server.
        /// </summary>
        void Stop();
    }
}