using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meepo.Core.Configs;
using Meepo.Serialization.Core.Events;

namespace Meepo.Serialization
{
    public interface ITypicalMeepo : IDisposable
    {
        /// <summary>
        /// Get IDs and addresses of all connected servers.
        /// </summary>
        /// <returns></returns>
        Dictionary<Guid, TcpAddress> GetServerClientInfos();

        /// <summary>
        /// Remove client.
        /// </summary>
        /// <param name="id">Client ID</param>
        void RemoveClient(Guid id);

        /// <summary>
        /// Send data to a specific client.
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <param name="data">Data to send</param>
        /// <returns></returns>
        Task Send<T>(Guid id, T data);

        /// <summary>
        /// Send data to a all clients.
        /// Including connected servers.
        /// </summary>
        /// <param name="data">Data to send</param>
        /// <returns></returns>
        Task Send<T>(T data);

        /// <summary>
        /// Run meepo server.
        /// Starts listening for new clients
        /// and connects to specified servers.
        /// </summary>
        Task Start();

        /// <summary>
        /// Stop meepo server.
        /// </summary>
        void Stop();

        /// <summary>
        /// Subscribe to a event when message of a specific type is received.
        /// </summary>
        /// <typeparam name="T">Meepo paskage</typeparam>
        /// <param name="action">MessageReceivedHandler delegate</param>
        void Subscribe<T>(MessageReceivedHandler<T> action);
    }
}