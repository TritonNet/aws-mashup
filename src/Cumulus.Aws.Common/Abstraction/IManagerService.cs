using System.Threading;
using System.Threading.Tasks;

namespace Cumulus.Aws.Common.Abstraction
{
    public interface IManagerService
    {
        Task RoutePendingObjects(CancellationToken cancellationToken);

        Task RouteUnQueuedObjects(CancellationToken cancellationToken);
    }
}
