using System.Net.Sockets;

namespace Meepo.Core.Configs
{
    public static class TcpClientConfig
    {
        private const int BufferSizeInBytes = 8192;

        public static void ApplyConfig(this TcpClient client)
        {
            client.ReceiveBufferSize = BufferSizeInBytes;
            client.SendBufferSize = BufferSizeInBytes;
        }
    }
}
    