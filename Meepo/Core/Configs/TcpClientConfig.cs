using System.Net.Sockets;

namespace Meepo.Core.Configs
{
    public static class TcpClientConfig
    {
        public static void ApplyConfig(this TcpClient client, MeepoConfig config)
        {
            client.ReceiveBufferSize = config.BufferSizeInBytes;
            client.SendBufferSize = config.BufferSizeInBytes;
        }
    }
}
    