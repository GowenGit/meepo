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
        private const int NoOfRetries = 20;
        private readonly TimeSpan retryDelay = TimeSpan.FromSeconds(1);
        private readonly TimeSpan clientPollingDelay = TimeSpan.FromSeconds(0.5);

        private readonly ILogger logger;
        private readonly CancellationToken cancellationToken;

        private readonly MessageReceivedHandler messageReceived;
        private readonly ClientConnectionFailed clientConnectionFailed;

        public Guid Id { get; }

        public TcpClient Client { get; private set; }

        public TcpAddress Address { get; }

        public bool IsToServer { get; }

        public bool Connected { get; private set; }

        public ClientWrapper(
            TcpClient client,
            ILogger logger,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            ClientConnectionFailed clientConnectionFailed)
        : this(logger, cancellationToken, messageReceived, clientConnectionFailed)
        {
            IsToServer = false;

            Client = client;

            Client.ApplyConfig();

            Connected = true;

            StartListening();
        }

        public ClientWrapper(
            TcpAddress address,
            ILogger logger,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            ClientConnectionFailed clientConnectionFailed) 
        : this(logger, cancellationToken, messageReceived, clientConnectionFailed)
        {
            Address = address;

            IsToServer = true;

            Connected = Connect();
        }

        private ClientWrapper(
            ILogger logger,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            ClientConnectionFailed clientConnectionFailed)
        {
            Id = Guid.NewGuid();

            this.logger = logger;
            this.cancellationToken = cancellationToken;
            this.messageReceived = messageReceived;
            this.clientConnectionFailed = clientConnectionFailed;
        }

        /// <summary>
        /// Try to connect to a TcpClient.
        /// If can't connect to a client, return false.
        /// Otherwise start listener and return true.
        /// and close connection. 
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

            Client.ApplyConfig();

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

                if (retries++ >= NoOfRetries - 1) break;

                logger.Warning($"Can't connect to {Address.IPAddress}:{Address.Port}." +
                               $" Will retry in {retryDelay.Seconds} seconds. Retry: {retries}");

                Thread.Sleep(retryDelay);
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

                        Thread.Sleep(clientPollingDelay);
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
