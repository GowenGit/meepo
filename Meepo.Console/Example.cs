using System.Net;
using Meepo.Core.Configs;
using Meepo.Core.Extensions;

namespace Meepo.Console
{
    public class Example
    {
        public void ExampleMethod()
        {
            // IP Address to expose
            var address = new TcpAddress(IPAddress.Loopback, 9201);

            // Nodes to connect to
            var serverAddresses = new[] { new TcpAddress(IPAddress.Loopback, 9200) };

            using (var meepo = new Meepo(address, serverAddresses))
            {
                meepo.Start();

                meepo.MessageReceived += x => System.Console.WriteLine(x.Bytes.Decode());

                while (true)
                {
                    var text = System.Console.ReadLine();

                    meepo.SendAsync(text).Wait();
                }
            }
        }
    }
}
