using System.Net;
using Meepo.Core.Configs;
using Meepo.Core.Extensions;
using Meepo.Core.Logging;

namespace Meepo.Console
{
    public class Program
    {
        private static IMeepoNode meepoNode;

        public static void Main()
        {
            var config = new MeepoConfig
            {
                BufferSizeInBytes = 1000,
                Logger = new ConsoleLogger()
            };

            var address = new TcpAddress(IPAddress.Loopback, 9201);
            var serverAddresses = new[] { new TcpAddress(IPAddress.Loopback, 9200)};

            using (meepoNode = new MeepoNode(address, serverAddresses, config))
            {
                meepoNode.Start();

                meepoNode.MessageReceived += OnMessageReceived;

                while (true)
                {
                    var text = System.Console.ReadLine();

                    if (text.ToLower() == "q") return;

                    meepoNode.SendAsync(text).Wait();
                }
            }
        }

        private static void OnMessageReceived(MessageReceivedEventArgs args)
        {
            System.Console.WriteLine($"Received: {args.Bytes.Decode()}");
            ShowServers();
        }

        private static void ShowServers()
        {
            var servers = meepoNode.GetServerClientInfos();

            foreach (var tcpAddress in servers)
            {
                System.Console.WriteLine($"ID: {tcpAddress.Key} URL: {tcpAddress.Value.IPAddress}:{tcpAddress.Value.Port}");
            }
        }
    }
}