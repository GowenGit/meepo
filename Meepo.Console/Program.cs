using System.Net;
using System.Text;
using Meepo.Core.Configs;
using Meepo.Core.Extensions;
using Meepo.Core.Logging;

namespace Meepo.Console
{
    public class Program
    {
        private static IMeepo meepo;

        public static void Main()
        {
            var config = new MeepoConfig
            {
                Logger = new ConsoleLogger()
            };

            var address = new TcpAddress(IPAddress.Loopback, 9200);
            var serverAddresses = new[] { new TcpAddress(IPAddress.Loopback, 9201)};

            using (meepo = new Meepo(address, serverAddresses, config))
            {
                meepo.Start();

                meepo.MessageReceived += OnMessageReceived;

                while (true)
                {
                    var text = System.Console.ReadLine();

                    if (text.ToLower() == "q") return;

                    meepo.SendAsync(text).Wait();
                }
            }
        }

        private static void OnMessageReceived(MessageReceivedEventArgs args)
        {
            System.Console.WriteLine($"Received: {Encoding.UTF8.GetString(args.Bytes)}");
            ShowServers();
        }

        private static void ShowServers()
        {
            var servers = meepo.GetServerClientInfos();

            foreach (var tcpAddress in servers)
            {
                System.Console.WriteLine($"ID: {tcpAddress.Key} URL: {tcpAddress.Value.IPAddress}:{tcpAddress.Value.Port}");
            }
        }
    }
}