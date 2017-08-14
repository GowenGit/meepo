using System.Threading;
using Meepo.Core.Client;

namespace Meepo.Core
{
    internal interface IClientManagerProvider
    {
        IClientManager GetClientManager(CancellationToken cancellationToken);
    }
}
