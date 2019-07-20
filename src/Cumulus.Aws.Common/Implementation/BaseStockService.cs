#region - References -
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3;
using Amazon.S3.Model;
using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.BusinessModels;
using Cumulus.Aws.Common.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Cumulus.Aws.Common.Implementation
{
    internal abstract class BaseStockService
    {
        #region - Private Properties -

        protected IRepository repository;

        protected IAmazonS3 amazonS3;

        protected IProductService productService;

        protected ICsvConverter csvConverter;

        protected IEncryptionService encryptionService;

        #endregion

        #region - Constructor -

        public BaseStockService(
            IRepository repository,
            IAmazonS3 amazonS3,
            IProductService productService,
            ICsvConverter csvConverter,
            IEncryptionService encryptionService)
        {
            this.repository = repository;
            this.amazonS3 = amazonS3;
            this.productService = productService;
            this.csvConverter = csvConverter;
            this.encryptionService = encryptionService;
        }

        #endregion

        #region - Protected Methods -

        protected async Task<PendingBucketObjectKeyCollection> GetPendingObjectKeyCollectionInternal(string lastObjectMarkerKey, StockMarket sourceBucketType, int maxNumberOfPendingObject, CancellationToken cancellationToken)
        {
            var pendingObjectKeyCollection = new PendingBucketObjectKeyCollection();

            pendingObjectKeyCollection.LastMarker = await this.repository.GetApplicationProperty(lastObjectMarkerKey, cancellationToken);

            var bucketName = this.GetBucketName(sourceBucketType);

            var listObjectResponse = default(ListObjectsResponse);

            var maxLoop = 100;
            var currentLoop = 0;

            do
            {
                var listObjectRequest = new ListObjectsRequest
                {
                    BucketName = bucketName,
                    MaxKeys = maxNumberOfPendingObject,
                    Marker = pendingObjectKeyCollection.LastMarker
                };

                listObjectResponse = await amazonS3.ListObjectsAsync(listObjectRequest, cancellationToken);

                var bucketObjectStatusCollection = await this.GetBucketObjectStatusCollection(sourceBucketType, listObjectResponse.S3Objects, cancellationToken);

                pendingObjectKeyCollection.AddRange(bucketObjectStatusCollection.Where(e => e.Status == (int)BucketObjectStatus.Unknown));

                pendingObjectKeyCollection.IsTruncated = listObjectResponse.IsTruncated;

                if (listObjectResponse.IsTruncated)
                    pendingObjectKeyCollection.LastMarker = listObjectResponse.NextMarker;

                currentLoop += 1;

            } while (
                pendingObjectKeyCollection.Count < maxNumberOfPendingObject && 
                listObjectResponse.S3Objects.Any() && 
                listObjectResponse.IsTruncated && 
                currentLoop < maxLoop);

            return pendingObjectKeyCollection;
        }

        #endregion

        #region - Public Methods -

        public async Task ImportObjectInternalAsync(StockMarket sourceBucketType, string objectKey, char csvSeperator, CancellationToken cancellationToken)
        {
            var objectID = this.encryptionService.GetBucketObjectHash(sourceBucketType, objectKey);

            var bucketName = this.GetBucketName(sourceBucketType);

            var bucketObject = await this.repository.GetItemAsync<BucketObject>(objectID, cancellationToken);

            if (bucketObject == null)
            {
                var objectMetaData = await amazonS3.GetObjectMetadataAsync(bucketName, objectKey, cancellationToken);

                bucketObject = new BucketObject
                {
                    ObjectID = objectID,
                    ObjectKey = objectKey,
                    SourceBucketType = (int)sourceBucketType,
                    Status = (int)BucketObjectStatus.Detected,
                    ObjectSize = objectMetaData.Headers.ContentLength,
                    CurrentStatusErrorCount = 0,
                    CurrentStatusLastError = null,
                    VersionNumber = null
                };
            }

            if (bucketObject.Status > (int)BucketObjectStatus.Queued)
                return;

            bucketObject.Status = (int)BucketObjectStatus.Processing;

            await this.repository.SaveItemAsync(bucketObject, cancellationToken);

            try
            {
                var objectStream = await amazonS3.GetObjectStreamAsync(bucketName, objectKey, null, cancellationToken);

                var enumerable = csvConverter.Convert<TradeActivity>(objectStream, csvSeperator);

                using (var bae = new BufferedAsyncEnumerable<TradeActivity>(enumerable, CumulusConstants.DynamoDbBatchWriteSize))
                {
                    while (await bae.MoveNextAsync())
                        await productService.SaveTraceActivityCollectionAsync(bae.Current, cancellationToken);
                }

                bucketObject.Status = (int)BucketObjectStatus.Processed;
            }
            catch (Exception exception)
            {
                bucketObject.CurrentStatusLastError = exception.Message;
                bucketObject.CurrentStatusErrorCount++;
                throw;
            }
            finally
            {
                await this.repository.SaveItemAsync(bucketObject, cancellationToken);
            }
        }

        public async Task<List<BucketObject>> GetBucketObjectStatusCollection(StockMarket sourceBucketType, ICollection<S3Object> objectCollection, CancellationToken cancellationToken)
        {
            var objectHashCollection = objectCollection
                                        .ToDictionary(
                                            @object => this.encryptionService.GetBucketObjectHash(sourceBucketType, @object.Key),
                                            @object => @object);

            var bucketObjectCollection = await this.repository.GetItemBatchAsync<BucketObject>(objectHashCollection.Keys, cancellationToken);

            foreach (var @object in objectHashCollection)
            {
                if (!bucketObjectCollection.Where(e => e.ObjectID == @object.Key).Any())
                {
                    bucketObjectCollection.Add(new BucketObject
                    {
                        ObjectID = @object.Key,
                        ObjectKey = @object.Value.Key,
                        SourceBucketType = (int)sourceBucketType,
                        Status = (int)BucketObjectStatus.Unknown,
                        ObjectSize = @object.Value.Size,
                        CurrentStatusErrorCount = 0,
                        CurrentStatusLastError = null,
                        VersionNumber = null
                    });
                }
            }

            return bucketObjectCollection;
        }

        #endregion

        #region - Private Methods -

        private string GetBucketName(StockMarket sourceBucketType)
        {
            switch (sourceBucketType)
            {
                case StockMarket.Eurex: return CumulusConstants.EurexBucketName;
                case StockMarket.Xetra: return CumulusConstants.XetraBucketName;
                default: throw new ArgumentException("Invalid Source Bucket Type", nameof(sourceBucketType));
            }
        }

        #endregion
    }
}
