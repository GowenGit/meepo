using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Meepo.Core;
using Meepo.Core.Client;
using Meepo.Core.Configs;
using Meepo.Core.Logging;
using Moq;
using NUnit.Framework;

namespace Meepo.Tests.Unit.Core
{
    [TestFixture]
    public class MeepoServerTests
    {
        private Mock<IClientManager> clientManager;
        private Mock<IClientManagerProvider> clientManagerProvider;
        private Mock<ILogger> logger;

        [SetUp]
        public void Initialize()
        {
            clientManager = new Mock<IClientManager>(MockBehavior.Strict);
            clientManagerProvider = new Mock<IClientManagerProvider>(MockBehavior.Strict);
            logger = new Mock<ILogger>(MockBehavior.Strict);

            logger.Setup(x => x.Message(It.IsAny<string>()));

            clientManagerProvider
                .Setup(x => x.GetClientManager(It.IsAny<CancellationToken>()))
                .Returns(clientManager.Object);
        }

        private MeepoServer GetSutObject()
        {
            return new MeepoServer(clientManagerProvider.Object, logger.Object);
        }

        [Test]
        public void StartServer_WhenCalled_ShouldStartListening()
        {
            var sut = GetSutObject();

            StartServer(sut);

            clientManager.Verify(x => x.Listen(), Times.Once);
        }

        [Test]
        public void SendToClient_WhenCalled_ShouldRouteMEssageToTheManager()
        {
            var id = Guid.NewGuid();
            var bytes = new byte[] { 00, 12, 23 };

            clientManager
                .Setup(x => x.SendToClient(id, bytes))
                .Returns(Task.CompletedTask);

            var sut = GetSutObject();

            StartServer(sut);

            var task = sut.SendToClient(id, bytes);

            task.Wait();

            clientManager.Verify(x => x.SendToClient(id, bytes), Times.Once);
        }

        [Test]
        public void SendToClients_WhenCalled_ShouldRouteMEssageToTheManager()
        {
            var bytes = new byte[] { 00, 12, 23 };

            clientManager
                .Setup(x => x.SendToClients(bytes))
                .Returns(Task.CompletedTask);

            var sut = GetSutObject();

            StartServer(sut);

            var task = sut.SendToClients(bytes);

            task.Wait();

            clientManager.Verify(x => x.SendToClients(bytes), Times.Once);
        }

        [Test]
        public void GetServerClientInfos_WhenCalled_ShouldRouteMEssageToTheManager()
        {
            clientManager
                .Setup(x => x.GetServerClientInfos())
                .Returns(new Dictionary<Guid, TcpAddress>());

            var sut = GetSutObject();

            StartServer(sut);

            sut.GetServerClientInfos();

            clientManager.Verify(x => x.GetServerClientInfos(), Times.Once);
        }

        private void StartServer(IMeepoServer server)
        {
            clientManager.Setup(x => x.Listen());

            var task = server.StartServer(CancellationToken.None);

            task.Wait();
        }
    }
}
