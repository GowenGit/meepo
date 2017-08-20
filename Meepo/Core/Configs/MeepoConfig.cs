using System;
using Meepo.Core.Logging;

namespace Meepo.Core.Configs
{
    public class MeepoConfig
    {
        public ILogger Logger { get; set; } = new SilentLogger();

        public int NumberOfRetries { get; set; } = 20;

        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

        public TimeSpan ClientPollingDelay { get; set; } = TimeSpan.FromMilliseconds(200);

        public int BufferSizeInBytes { get; set; } = 8192;
    }
}
