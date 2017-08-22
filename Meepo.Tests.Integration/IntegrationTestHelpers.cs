using System.Net;
using Meepo.Core.Configs;
using Meepo.Core.Logging;

namespace Meepo.Tests.Integration
{
    internal static class IntegrationTestHelpers
    {
        public const int WaitTime = 1000;

        public static Meepo StartServer(int port, ILogger logger, MessageReceivedHandler messageReceived, int buffer = 1000)
        {
            var address = new TcpAddress(IPAddress.Loopback, port);

            var config = new MeepoConfig
            {
                Logger = logger,
                BufferSizeInBytes = buffer
            };

            var tmp = new Meepo(address, config);

            tmp.MessageReceived += messageReceived;

            tmp.Start();

            return tmp;
        }

        public static Meepo StartClient(int port, int serverPort, ILogger logger, MessageReceivedHandler messageReceived, int buffer = 1000)
        {
            var address = new TcpAddress(IPAddress.Loopback, port);

            var serverAddress = new [] { new TcpAddress(IPAddress.Loopback, serverPort) };

            var config = new MeepoConfig
            {
                Logger = logger,
                BufferSizeInBytes = buffer
            };

            var tmp = new Meepo(address, serverAddress, config);

            tmp.MessageReceived += messageReceived;

            tmp.Start();

            return tmp;
        }
    }
}
