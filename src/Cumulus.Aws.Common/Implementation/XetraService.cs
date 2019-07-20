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
    internal class XetraService : BaseStockService, IXetraService
    {
        #region - Constructor -

        public XetraService(
            IRepository repository,
            IProductService productService,
            IAmazonS3 amazonS3,
            ICsvConverter csvConverter,
            IEncryptionService encryptionService)
            : base(repository, amazonS3, productService, csvConverter, encryptionService)
        {
        }

        #endregion

        #region - Public Methods | IXetraService -

        public async Task ImportObjectAsync(string objectKey, CancellationToken cancellationToken)
        {
            await base.ImportObjectInternalAsync(StockMarket.Xetra, objectKey, CumulusConstants.XetraCsvSeperator, cancellationToken);
        }

        public async Task<PendingBucketObjectKeyCollection> GetPendingObjectKeyCollection(int maxNumberOfPendingObject, CancellationToken cancellationToken)
        {
            return await base.GetPendingObjectKeyCollectionInternal(
                                CumulusConstants.ApplicationProperty.LastXetraObjectMarker,
                                StockMarket.Xetra,
                                maxNumberOfPendingObject,
                                cancellationToken);
        }

        public async Task UpdateLastReadMarker(string lastMarker, CancellationToken cancellationToken)
        {
            await this.repository.SetApplicationProperty(CumulusConstants.ApplicationProperty.LastXetraObjectMarker, lastMarker, cancellationToken);
        }

        #endregion
    }
}
