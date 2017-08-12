using System.Net;
using System.Text;
using Meepo.Core.Configs;
using Meepo.Core.Helpers;

namespace Meepo.Console
{
    public class Program
    {
        private static IMeepo meepo;

        public static void Main()
        {
            //var address = new TcpAddress(IPAddress.Loopback, 9200);
            //meepo = new Meepo(address);

            //var address = new TcpAddress(IPAddress.Loopback, 9201);
            //var serverAddress = new TcpAddress(IPAddress.Loopback, 9200);
            //meepo = new Meepo(address, new[] { serverAddress });

            var address = new TcpAddress(IPAddress.Loopback, 9202);
            var serverAddress1 = new TcpAddress(IPAddress.Loopback, 9200);
            var serverAddress2 = new TcpAddress(IPAddress.Loopback, 9201);
            meepo = new Meepo(address, new[] { serverAddress1, serverAddress2 });

            meepo.RunServer();

            meepo.MessageReceived += OnMessageReceived;

            while (true)
            {
                var text = System.Console.ReadLine();

                if (text.ToLower() == "q")
                {
                    meepo.StopServer();
                    break;
                }

                var task = meepo.SendToClients(Encoding.UTF8.GetBytes(text));

                task.Wait();
            }
        }

        private static void OnMessageReceived(object sender, MessageReceivedEventArgs args)
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