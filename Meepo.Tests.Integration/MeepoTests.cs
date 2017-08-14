using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Meepo.Core.Configs;
using Meepo.Core.Helpers;
using Moq;

namespace Meepo.Tests.Integration
{
    [TestFixture]
    public class MeepoTests
    {
        private Mock<ILogger> clientLogger;
        private Mock<ILogger> serverLogger;

        private string message;

        private readonly Meepo server;
        private readonly Meepo client;

        public MeepoTests()
        {
            InitializeLoggers();

            server = StartServer();
            client = StartClient();

            Thread.Sleep(1000);
        }

        private void InitializeLoggers()
        {
            clientLogger = new Mock<ILogger>(MockBehavior.Strict);

            clientLogger.Setup(x => x.Message(It.IsAny<string>()));

            serverLogger = new Mock<ILogger>(MockBehavior.Strict);

            serverLogger.Setup(x => x.Message(It.IsAny<string>()));
        }

        private Meepo StartServer()
        {
            var address = new TcpAddress(IPAddress.Loopback, 9211);

            var tmp = new Meepo(address, serverLogger.Object);

            tmp.MessageReceived += OnMessageReceived;

            tmp.Start();

            return tmp;
        }

        private Meepo StartClient()
        {
            var address = new TcpAddress(IPAddress.Loopback, 9212);

            var serverAddress = new TcpAddress(IPAddress.Loopback, 9211);

            var tmp = new Meepo(address, new[] { serverAddress }, clientLogger.Object);

            tmp.MessageReceived += OnMessageReceived;

            tmp.Start();

            return tmp;
        }

        [SetUp]
        public void Initialize()
        {
            message = "";
        }

        [Test]
        public void Meepo_WhenServerStarted_ClientShouldBeAbleToConnect()
        {
            serverLogger.Verify(x => x.Message("Connection accepted"), Times.Once);

            clientLogger.Verify(x => x.Message("Connection accepted from 127.0.0.1:9211"), Times.Once);
        }

        [Test]
        public void Sent_WhenCalled_ServerShouldGetTheMessage()
        {
            const string text = "Hello!";

            var task = client.Send(ToBytes(text));

            task.Wait();

            Thread.Sleep(1000);

            Assert.AreEqual(text, message);
        }

        [Test]
        public void Sent_WhenCalled_ClientShouldGetTheMessage()
        {
            const string text = "Hi!";

            var task = server.Send(ToBytes(text));

            task.Wait();

            Thread.Sleep(1000);

            Assert.AreEqual(text, message);
        }

        private void OnMessageReceived(MessageReceivedEventArgs args)
        {
            message = Encoding.UTF8.GetString(args.Bytes);
        }

        private static byte[] ToBytes(string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            server.Stop();
            client.Stop();
        }
    }
}
