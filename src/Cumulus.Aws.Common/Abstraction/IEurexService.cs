using Cumulus.Aws.Common.BusinessModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cumulus.Aws.Common.Abstraction
{
    public interface IEurexService
    {
        Task ImportObjectAsync(string objectKey, CancellationToken cancellationToken);

        Task<PendingBucketObjectKeyCollection> GetPendingObjectKeyCollection(int maxNumberOfPendingObject, CancellationToken cancellationToken);

        Task UpdateLastReadMarker(string lastMarker, CancellationToken cancellationToken);
    }
}
