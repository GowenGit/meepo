using System.Net.Sockets;

namespace Meepo.Core.Configs
{
    /// <summary>
    /// Todo: Make JSON configurable.
    /// </summary>
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
    