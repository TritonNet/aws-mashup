using Cumulus.Aws.Common.BusinessModels;
using System.Threading;
using System.Threading.Tasks;

namespace Cumulus.Aws.Common.Abstraction
{
    public interface IXetraService
    {
        Task<PendingBucketObjectKeyCollection> GetPendingObjectKeyCollection(int maxNumberOfPendingObject, CancellationToken cancellationToken);

        Task ImportObjectAsync(string objectKey, CancellationToken cancellationToken);

        Task UpdateLastReadMarker(string lastMarker, CancellationToken cancellationToken);
    }
}
