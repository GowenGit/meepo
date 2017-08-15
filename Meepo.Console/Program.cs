using System.Net;
using System.Text;
using Meepo.Core.Configs;
using Meepo.Core.Extensions;

namespace Meepo.Console
{
    public class Program
    {
        private static IMeepo meepo;

        public static void Main()
        {
            //var address = new TcpAddress(IPAddress.Loopback, 9200);
            //meepo = new Meepo(address);

            var address = new TcpAddress(IPAddress.Loopback, 9201);
            var serverAddresses = new[] { new TcpAddress(IPAddress.Parse("192.168.15.123"), 9201)};

            using (meepo = new Meepo(address, serverAddresses))
            {
                meepo.Start();

                meepo.MessageReceived += OnMessageReceived;

                while (true)
                {
                    var text = System.Console.ReadLine();

                    if (text.ToLower() == "q") return;

                    meepo.Send(text).Wait();
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
                System.Console.WriteLine($"Id: {tcpAddress.Key} Url: {tcpAddress.Value.IPAddress}:{tcpAddress.Value.Port}");
            }
        }
    }
}