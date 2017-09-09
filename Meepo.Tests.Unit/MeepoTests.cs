using System;
using System.Net;
using System.Threading;
using Meepo.Core;
using Meepo.Core.Configs;
using Meepo.Core.Logging;
using Meepo.Core.StateMachine;
using Moq;
using NUnit.Framework;

// ReSharper disable ObjectCreationAsStatement

namespace Meepo.Tests.Unit
{
    [TestFixture]   
    public class MeepoTests
    {
        private MeepoStateMachine stateMachine;

        private Mock<IMeepoServer> server;
        private Mock<ILogger> logger;

        [SetUp]
        public void Initialize()
        {
            logger = new Mock<ILogger>();

            stateMachine = new MeepoStateMachine(logger.Object);

            server = new Mock<IMeepoServer>();
        }

        private MeepoNode GetSutObject()
        {
            return new MeepoNode(stateMachine, server.Object);
        }

        [Test]
        public void TcpAddress_WhenInitializedWWithNull_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new TcpAddress(null, 80));
        }

        [Test]
        public void Ctor_WhenCalledWithNullConfig_ShouldThrowException()
        {
            var listenerAddress = new TcpAddress(IPAddress.Any, 80);
            var serverAddresses = new TcpAddress[0];

            Assert.Throws<ArgumentNullException>(() => new MeepoNode(listenerAddress, config: null));
            Assert.Throws<ArgumentNullException>(() => new MeepoNode(listenerAddress, serverAddresses, null));
        }

        [Test]
        public void Ctor_WhenCalledWithNullServers_ShouldThrowException()
        {
            var listenerAddress = new TcpAddress(IPAddress.Any, 80);
            var config = new MeepoConfig();

            Assert.Throws<ArgumentNullException>(() => new MeepoNode(listenerAddress, serverAddresses: null));
            Assert.Throws<ArgumentNullException>(() => new MeepoNode(listenerAddress, null, config));
        }

        [Test]
        public void Ctor_WhenCalledWithNullLogger_ShouldThrowException()
        {
            var listenerAddress = new TcpAddress(IPAddress.Any, 80);
            var serverAddresses = new TcpAddress[0];
            var config = new MeepoConfig
            {
                Logger = null
            };

            Assert.Throws<ArgumentNullException>(() => new MeepoNode(listenerAddress, config));
            Assert.Throws<ArgumentNullException>(() => new MeepoNode(listenerAddress, serverAddresses, config));
        }

        [Test]
        public void Start_WhenCalled_ShouldStartTheServer()
        {
            var sut = GetSutObject();

            sut.Start();

            server.Verify(x => x.StartServer(It.IsAny<CancellationToken>()));
        }

        [Test]
        public void Start_WhenCalledTwice_ShouldNotStartTheServer()
        {
            var sut = GetSutObject();

            sut.Start();
            sut.Start();

            server.Verify(x => x.StartServer(It.IsAny<CancellationToken>()), Times.Once);

            logger.Verify(x => x.Warning(It.IsAny<string>()));
        }

        [Test]
        public void Meepo_WhenNotRunning_ShouldHaveStoppedState()
        {
            var sut = GetSutObject();

            Assert.AreEqual(State.Stopped, sut.ServerState);
        }

        [Test]
        public void Meepo_WhenRunning_ShouldHaveRunningState()
        {
            var sut = GetSutObject();

            sut.Start();

            Assert.AreEqual(State.Running, sut.ServerState);
        }

        [Test]
        public void Meepo_WhenStopped_ShouldHaveStoppedState()
        {
            var sut = GetSutObject();

            Assert.AreEqual(State.Stopped, sut.ServerState);

            sut.Start();

            Assert.AreEqual(State.Running, sut.ServerState);

            sut.Stop();

            Assert.AreEqual(State.Stopped, sut.ServerState);
        }

        [Test]
        public void RemoveClient_WhenCalledWithEmptyGuid_ShouldThrow()
        {
            var sut = GetSutObject();

            Assert.Throws<ArgumentException>(() => sut.RemoveClient(Guid.Empty));
        }

        [Test]
        public void RemoveClient_WhenCalledOnStopped_ShouldNotCallServer()
        {
            var sut = GetSutObject();

            var id = Guid.NewGuid();

            sut.RemoveClient(id);

            server.Verify(x => x.RemoveClient(id), Times.Never);

            logger.Verify(x => x.Warning(It.IsAny<string>()));
        }

        [Test]
        public void RemoveClient_WhenCalledOnRunning_ShouldCallServer()
        {
            var sut = GetSutObject();

            var id = Guid.NewGuid();

            sut.Start();

            sut.RemoveClient(id);

            server.Verify(x => x.RemoveClient(id));
        }

        [Test]
        public void SendAsync_WhenCalledWithEmptyGuid_ShouldThrow()
        {
            var sut = GetSutObject();

            Assert.Throws<ArgumentException>(() => sut.SendAsync(Guid.Empty, new byte[0]).GetAwaiter().GetResult());
        }


        [Test]
        public void SendAsync_WhenCalledWithNullBytes_ShouldThrow()
        {
            var sut = GetSutObject();

            Assert.Throws<ArgumentNullException>(() => sut.SendAsync(Guid.NewGuid(), null).GetAwaiter().GetResult());
        }

        [Test]
        public void SendAsync_WhenCalledOnStopped_ShouldNotCallServer()
        {
            var sut = GetSutObject();

            var id = Guid.NewGuid();
            var bytes = new byte[0];

            sut.SendAsync(id, bytes).Wait();

            server.Verify(x => x.SendToClientAsync(id, bytes), Times.Never);

            logger.Verify(x => x.Warning(It.IsAny<string>()));
        }

        [Test]
        public void SendAsync_WhenCalledOnRunning_ShouldCallServer()
        {
            var sut = GetSutObject();

            var id = Guid.NewGuid();
            var bytes = new byte[0];

            sut.Start();

            sut.SendAsync(id, bytes).Wait();

            server.Verify(x => x.SendToClientAsync(id, bytes));
        }

        [Test]
        public void BcastSendAsync_WhenCalledWithNullBytes_ShouldThrow()
        {
            var sut = GetSutObject();

            Assert.Throws<ArgumentNullException>(() => sut.SendAsync(null).GetAwaiter().GetResult());
        }

        [Test]
        public void BcasSendAsync_WhenCalledOnStopped_ShouldNotCallServer()
        {
            var sut = GetSutObject();

            var bytes = new byte[0];

            sut.SendAsync(bytes).Wait();

            server.Verify(x => x.SendToClientsAsync(bytes), Times.Never);

            logger.Verify(x => x.Warning(It.IsAny<string>()));
        }

        [Test]
        public void BcasSendAsync_WhenCalledOnRunning_ShouldCallServer()
        {
            var sut = GetSutObject();

            var bytes = new byte[0];

            sut.Start();

            sut.SendAsync(bytes).Wait();

            server.Verify(x => x.SendToClientsAsync(bytes));
        }

        [Test]
        public void GetServerClientInfos_WhenCalledOnStopped_ShouldNotCallServer()
        {
            var sut = GetSutObject();

            sut.GetServerClientInfos();

            server.Verify(x => x.GetServerClientInfos(), Times.Never);

            logger.Verify(x => x.Warning(It.IsAny<string>()));
        }

        [Test]
        public void GetServerClientInfos_WhenCalledOnRunning_ShouldCallServer()
        {
            var sut = GetSutObject();

            sut.Start();

            sut.GetServerClientInfos();

            server.Verify(x => x.GetServerClientInfos());
        }
    }
}
