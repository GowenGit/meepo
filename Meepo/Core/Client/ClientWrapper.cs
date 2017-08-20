using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core.Configs;
using Meepo.Core.Logging;
using Meepo.Util;

namespace Meepo.Core.Client
{
    internal class ClientWrapper : IHasIndex
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

            Connected = Connect();
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

        /// <summary>
        /// Try to connect to a TcpClient.
        /// If can't connect to a client, return false
        /// and close connection. 
        /// Otherwise start listener and return true.
        /// </summary>
        /// <returns></returns>
        private bool Connect()
        {
            if (!IsToServer)
            {
                Close();
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
                    var task = Client.ConnectAsync(Address.IPAddress, Address.Port);

                    task.Wait(cancellationToken);

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

                Thread.Sleep(config.RetryDelay);
            }

            logger.Error("Error while connecting to the client", failedToConnectException);

            Close();

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
                    while (Connected)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        if (!Client.Connected) Connected = Connect();

                        while (Connected && stream.DataAvailable && !cancellationToken.IsCancellationRequested)
                        {
                            var bytes = new byte[Client.Available];

                            await stream.ReadAsync(bytes, 0, bytes.Length, cancellationToken);

                            var args = new MessageReceivedEventArgs(Id, bytes);

                            messageReceived?.Invoke(args);
                        }

                        Thread.Sleep(config.ClientPollingDelay);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Oops! Something went wrong! Will try to reconnect...", ex);

                Connected = Connect();
            }
        }

        public async Task Send(byte[] bytes)
        {
            try
            {
                var stream = Client.GetStream();

                await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Error("Oops! Something went wrong!", ex);
            }
        }

        public void Close()
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
