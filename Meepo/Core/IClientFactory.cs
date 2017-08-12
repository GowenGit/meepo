using System.Net.Sockets;
using Meepo.Core.Configs;

namespace Meepo.Core
{
    internal interface IClientFactory
    {
        ClientWrapper GetClient(TcpAddress address);

        ClientWrapper GetClient(TcpClient client);
    }
}