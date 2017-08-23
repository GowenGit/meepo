using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core.Configs;
using Meepo.Core.Exceptions;
using Meepo.Core.Logging;
using Meepo.Util;

namespace Meepo.Core.Client
{
    internal class ClientWrapper : IHasIndex, IDisposable
    {
        private readonly ILogger logger;
        private readonly MeepoConfig config;
        private readonly CancellationToken cancellationToken;

        private readonly MessageReceivedHandler messageReceived;
        private readonly ClientConnectionFailed clientConnectionFailed;

        private bool Connected { get; set; }

        private TcpClient Client { get; set; }

        public Guid Id { get; }

        public TcpAddress Address { get; }

        public bool IsToServer { get; }

        #region Constructors

        public ClientWrapper(
            TcpClient client,
            MeepoConfig config,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            ClientConnectionFailed clientConnectionFailed)
        : this(config, cancellationToken, messageReceived, clientConnectionFailed)
        {
            IsToServer = false;

            Client = client;

            Client.ApplyConfig(config);

            Connected = true;

            StartListening();
        }

        public ClientWrapper(
            TcpAddress address,
            MeepoConfig config,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            ClientConnectionFailed clientConnectionFailed)
        : this(config, cancellationToken, messageReceived, clientConnectionFailed)
        {
            Address = address;

            IsToServer = true;

            Connected = Connect().Result;
        }

        private ClientWrapper(
            MeepoConfig config,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            ClientConnectionFailed clientConnectionFailed)
        {
            Id = Guid.NewGuid();

            logger = config.Logger;

            this.config = config;
            this.cancellationToken = cancellationToken;
            this.messageReceived = messageReceived;
            this.clientConnectionFailed = clientConnectionFailed;
        } 

        #endregion

        /// <summary>
        /// Try to connect to a TcpClient.
        /// If can't connect to a client, return false
        /// and close connection. 
        /// Otherwise start listener and return true.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Connect()
        {
            if (!IsToServer)
            {
                Dispose();
                return false;
            }

            var retries = 0;

            var failedToConnectException = new Exception();

            Client = new TcpClient();

            Client.ApplyConfig(config);

            while (!Client.Connected)
            {
                try
                {
                    await Client.ConnectAsync(Address.IPAddress, Address.Port);

                    if (Client.Connected)
                    {
                        StartListening();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    failedToConnectException = ex;
                }

                if (retries++ >= config.NumberOfRetries - 1) break;

                logger.Warning($"Can't connect to {Address.IPAddress}:{Address.Port}." +
                               $" Will retry in {config.RetryDelay.Seconds} seconds. Retry: {retries}");

                await Task.Delay(config.RetryDelay, cancellationToken);
            }

            logger.Error("Error while connecting to the client", failedToConnectException);

            Dispose();

            return false;
        }

        private void StartListening()
        {
            var thread = new Thread(Listen);

            logger.Message(IsToServer ? $"Connection accepted from {Address.IPAddress}:{Address.Port}" : "Connection accepted");

            thread.Start();
        }

        private async void Listen()
        {
            try
            {
                using (var stream = Client.GetStream())
                {
                    var messageBufferReader = new MessageBufferReader(config, Client, messageReceived);

                    while (Connected)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        if (!Client.Connected)
                        {
                            Connected = await Connect();
                            messageBufferReader = new MessageBufferReader(config, Client, messageReceived);
                        }

                        while (Connected && stream.DataAvailable && !cancellationToken.IsCancellationRequested)
                        {
                            await messageBufferReader.Read(stream, cancellationToken, Id);
                        }

                        await Task.Delay(config.ClientPollingDelay, CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Oops! Something went wrong! Will try to reconnect...", ex);

                Connected = await Connect();
            }
        }

        /// <summary>
        /// Send first 4 bytes indicating message size
        /// followed by the message itself.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public async Task Send(byte[] bytes)
        {
            try
            {
                if (bytes.Length > config.BufferSizeInBytes)
                {
                    throw new MeepoException($"Buffer size {config.BufferSizeInBytes} (bytes) is less than the message size {bytes.Length} (bytes)!");
                }

                var stream = Client.GetStream();

                await stream.WriteAsync(BitConverter.GetBytes(bytes.Length), 0, 4, cancellationToken);
                await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
            }
            catch (Exception ex) when (!(ex is MeepoException))
            {
                logger.Error("Oops! Something went wrong!", ex);
            }
        }

        public void Dispose()
        {
            try
            {
                clientConnectionFailed(Id);

                Close(Client);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to dispose the client!", ex);

                throw;
            }
        }

        private static void Close(TcpClient client)
        {
            if (client != null && client.Connected)
            {
                client.GetStream()?.Dispose();
            }

            client?.Dispose();
        }
    }
}
