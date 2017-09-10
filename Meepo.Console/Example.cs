using System.Net;
using Meepo.Core.Configs;
using Meepo.Core.Extensions;
using Meepo.Core.Logging;

namespace Meepo.Console
{
    public class Example
    {
        public void ExampleMethod()
        {
            var config = new MeepoConfig
            {
                Logger = new ConsoleLogger()
            };

            // IP Address to expose
            var address = new TcpAddress(IPAddress.Loopback, 9201);

            // Nodes to connect to
            var serverAddresses = new[] { new TcpAddress(IPAddress.Loopback, 9200) };

            using (var meepo = new MeepoNode(address, serverAddresses, config))
            {
                meepo.Start();

                meepo.MessageReceived += x => System.Console.WriteLine(x.Bytes.Decode());

                while (true)
                {
                    var text = System.Console.ReadLine();

                    if (text.ToLower() == "q") break;

                    meepo.SendAsync(text).Wait();
                }
            }
        }
    }
}
