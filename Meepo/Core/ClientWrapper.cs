using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core.Configs;
using Meepo.Core.Helpers;
using Meepo.Util;

namespace Meepo.Core
{
    internal class ClientWrapper : IHasIndex
    {
        private const int NoOfRetries = 20;
        private readonly TimeSpan retryDelay = TimeSpan.FromSeconds(1);
        private readonly TimeSpan clientPollingDelay = TimeSpan.FromSeconds(0.5);

        private readonly ILogger logger;
        private readonly CancellationToken cancellationToken;
        private readonly MessageReceivedHandler messageReceived;

        private readonly Action<Guid> clientConnectionFailed;

        public Guid Id { get; }

        public TcpClient Client { get; private set; }

        public TcpAddress Address { get; }

        public bool IsToServer { get; }

        public ClientWrapper(
            TcpClient client,
            ILogger logger,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            Action<Guid> clientConnectionFailed)
        : this(logger, cancellationToken, messageReceived, clientConnectionFailed)
        {
            IsToServer = false;

            Client = client;

            var thread = new Thread(Listen);

            logger.Message("Connection accepted.");

            thread.Start();
        }

        public ClientWrapper(
            TcpAddress address,
            ILogger logger,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            Action<Guid> clientConnectionFailed) 
        : this(logger, cancellationToken, messageReceived, clientConnectionFailed)
        {
            Address = address;

            IsToServer = true;

            Client = Connect();
        }

        private ClientWrapper(
            ILogger logger,
            CancellationToken cancellationToken,
            MessageReceivedHandler messageReceived,
            Action<Guid> clientConnectionFailed)
        {
            Id = Guid.NewGuid();

            this.logger = logger;
            this.cancellationToken = cancellationToken;
            this.messageReceived = messageReceived;
            this.clientConnectionFailed = clientConnectionFailed;
        }

        private TcpClient Connect()
        {
            if (!IsToServer)
            {
                Close();
                return null;
            }

            var client = new TcpClient();

            var retries = 1;

            var failedToConnectException = new Exception();

            while (!client.Connected)
            {
                try
                {
                    var task = client.ConnectAsync(Address.IPAddress, Address.Port);

                    task.Wait(cancellationToken);
                }
                catch (Exception ex)
                {
                    failedToConnectException = ex;
                }

                if (client.Connected) continue;

                logger.Warning($"Can't connect to {Address.IPAddress}:{Address.Port}." +
                               $" Will retry in {retryDelay} seconds. Retry: {retries}");

                if (retries++ > NoOfRetries - 1) break;
  
                Thread.Sleep(retryDelay);
            }

            if (!client.Connected)
            {
                logger.Error("Error while connecting to the client", failedToConnectException);
                Close();
                return null;
            }

            var thread = new Thread(Listen);

            logger.Message($"Connection accepted from {Address.IPAddress}:{Address.Port}");

            thread.Start();

            return client;
        }

        private async void Listen()
        {
            try
            {
                Client.ApplyConfig();

                using (var stream = Client.GetStream())
                {
                    while (Client != null)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        if (!Client.Connected) Client = Connect();

                        while (Client != null && stream.DataAvailable && !cancellationToken.IsCancellationRequested)
                        {
                            var bytes = new byte[Client.Available];

                            await stream.ReadAsync(bytes, 0, bytes.Length, cancellationToken);

                            var args = new MessageReceivedEventArgs(Id, bytes);

                            messageReceived?.Invoke(this, args);
                        }

                        Thread.Sleep(clientPollingDelay);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Oops! Something went wrong! Will try to reconnect...", ex);

                Client = Connect();
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

                if (Client != null && Client.Connected)
                {
                    Client?.GetStream()?.Dispose();
                }

                Client?.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to dispose the client!", ex);

                throw;
            }
        }
    }
}
