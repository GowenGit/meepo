using System;
using System.Threading.Tasks;
using Meepo.Core.Extensions;
using Meepo.Core.Logging;
using Moq;
using NUnit.Framework;

namespace Meepo.Tests.Integration
{
    [TestFixture]
    [NonParallelizable]
    public class MeepoByteTests
    {
        private Mock<ILogger> clientLogger;
        private Mock<ILogger> serverLogger;

        private byte[] bytes;

        private readonly Meepo server;
        private readonly Meepo client;

        public MeepoByteTests()
        {
            InitializeLoggers();

            server = IntegrationTestHelpers.StartServer(9313, serverLogger.Object, OnMessageReceived);
            client = IntegrationTestHelpers.StartClient(9314, 9313, clientLogger.Object, OnMessageReceived, 10000);

            Task.Delay(IntegrationTestHelpers.WaitTime).Wait();
        }

        private void InitializeLoggers()
        {
            clientLogger = new Mock<ILogger>(MockBehavior.Strict);

            clientLogger.Setup(x => x.Message(It.IsAny<string>()));

            serverLogger = new Mock<ILogger>(MockBehavior.Strict);

            serverLogger.Setup(x => x.Message(It.IsAny<string>()));
        }

        [SetUp]
        public void Initialize()
        {
            bytes = new byte[3];
        }

        [Test]
        public void Meepo_WhenServerStarted_ClientShouldBeAbleToConnect()
        {
            serverLogger.Verify(x => x.Message("Connection accepted"), Times.Once);

            clientLogger.Verify(x => x.Message("Connection accepted from 127.0.0.1:9313"), Times.Once);
        }

        [Test]
        public void SendAsync_WhenCalledWithZeroBytes_ServerShouldGetZeroBytes()
        {
            client.SendAsync(new byte[0]).Wait();

            Task.Delay(IntegrationTestHelpers.WaitTime).Wait();

            Assert.AreEqual(0, bytes.Length);
        }

        [Test]
        public void SendAsync_WhenCalledWithNBytes_ServerShouldGetNBytes()
        {
            client.SendAsync(new byte[100]).Wait();

            Task.Delay(IntegrationTestHelpers.WaitTime).Wait();

            Assert.AreEqual(100, bytes.Length);
        }

        [Test]
        public void SendAsync_WhenCalledWithBufferSizeBytes_ServerShouldReturnBufferSizeBytes()
        {
            client.SendAsync(new byte[1000]).Wait();

            Task.Delay(IntegrationTestHelpers.WaitTime).Wait();

            Assert.AreEqual(1000, bytes.Length);
        }

        [Test]
        public void SendAsync_WhenCalledWithBufferSizeBytes_ClientShouldReturnBufferSizeBytes()
        {
            server.SendAsync(new byte[1000]).Wait();

            Task.Delay(IntegrationTestHelpers.WaitTime).Wait();

            Assert.AreEqual(1000, bytes.Length);
        }

        [Test]
        public void SendAsync_WhenCalledWithMoreThanBufferSizeBytes_ServerShouldThrow()
        {
            Assert.Throws<AggregateException>(() => server.SendAsync(new byte[1001]).Wait());
        }

        [Test]
        public void SendAsync_WhenCalledWithMoreThanBufferSizeBytes_ClientShouldThrow()
        {
            Assert.Throws<AggregateException>(() => client.SendAsync(new byte[10001]).Wait());
        }

        private void OnMessageReceived(MessageReceivedEventArgs args)
        {
            bytes = args.Bytes;
        }

        [Test]
        public void SendAsync_WhenCalled_ServerShouldGetTheMessage()
        {
            const string text = "Hello!";

            client.SendAsync(text).Wait();

            Task.Delay(IntegrationTestHelpers.WaitTime).Wait();

            Assert.AreEqual(text, bytes.Decode());
        }

        [Test]
        public void SendAsync_WhenCalled_ClientShouldGetTheMessage()
        {
            const string text = "Hi!";

            server.SendAsync(text).Wait();

            Task.Delay(IntegrationTestHelpers.WaitTime).Wait();

            Assert.AreEqual(text, bytes.Decode());
        }

        [Test]
        public void SendAsync_WhenCalledWithInvalidId_ShouldThrow()
        {
            Assert.Throws<AggregateException>(() => server.SendAsync(Guid.NewGuid(), "").Wait());
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            server.Stop();
            client.Stop();
        }
    }
}
