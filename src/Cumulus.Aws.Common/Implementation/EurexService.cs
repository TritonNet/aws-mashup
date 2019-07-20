#region - References -
using Amazon.S3;
using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.BusinessModels;
using Cumulus.Aws.Common.Infrastructure;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Cumulus.Aws.Common.Implementation
{
    internal class EurexService : BaseStockService, IEurexService
    {
        #region - Private Properties -

        #endregion

        #region - Constructor -

        public EurexService(
            IProductService productService,
            IAmazonS3 amazonS3,
            ICsvConverter csvConverter,
            IRepository repository,
            IEncryptionService encryptionService)
            : base(repository, amazonS3, productService, csvConverter, encryptionService)
        {

        }

        #endregion

        #region - Public Methods | IEurexService -

        public async Task ImportObjectAsync(string objectKey, CancellationToken cancellationToken)
        {
            await base.ImportObjectInternalAsync(StockMarket.Eurex, objectKey, CumulusConstants.EurexCsvSeperator, cancellationToken);
        }

        public async Task<PendingBucketObjectKeyCollection> GetPendingObjectKeyCollection(int maxNumberOfPendingObject, CancellationToken cancellationToken)
        {
            return await base.GetPendingObjectKeyCollectionInternal(
                                CumulusConstants.ApplicationProperty.LastEurexObjectMarker,
                                StockMarket.Eurex,
                                maxNumberOfPendingObject,
                                cancellationToken);
        }

        public async Task UpdateLastReadMarker(string lastMarker, CancellationToken cancellationToken)
        {
            await this.repository.SetApplicationProperty(CumulusConstants.ApplicationProperty.LastEurexObjectMarker, lastMarker, cancellationToken);
        }

        #endregion
    }
}
