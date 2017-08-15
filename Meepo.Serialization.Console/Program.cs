using System;
using System.Net;
using System.Reflection;
using Meepo.Core.Configs;
using Meepo.Serialization.Core.Attributes;

namespace Meepo.Serialization.Console
{
    class Program
    {
        private static ITypicalMeepo meepo;

        public static void Main()
        {
            var address = new TcpAddress(IPAddress.Loopback, 9200);
            var serverAddresses = new[] { new TcpAddress(IPAddress.Loopback, 9201) };

            using (meepo = new TypicalMeepo(address, serverAddresses, new []{ Assembly.GetEntryAssembly() }))
            {
                meepo.Start();

                meepo.Subscribe<Info>(OnInfoReceived);

                var info = new Info
                {
                    Date = DateTime.Now,
                    Message = "Hello there!"
                };

                while (true)
                {
                    var text = System.Console.ReadLine();

                    if (text.ToLower() == "q") return;

                    meepo.Send(info).Wait();
                }
            }
        }

        public static void OnInfoReceived(Guid id, Info info)
        {
            System.Console.WriteLine($"Client ID: {id}");
            System.Console.WriteLine($"Date: {info.Date}");
            System.Console.WriteLine($"Message: {info.Message}");
        }
    }

    [MeepoPackage(1)]
    public class Info
    {
        public DateTime Date { get; set; }

        public string Message { get; set; }
    }
}