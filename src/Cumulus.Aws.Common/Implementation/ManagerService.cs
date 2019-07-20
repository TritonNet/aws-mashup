#region - References -
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
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
    internal class ManagerService : IManagerService
    {
        #region - Private Properties -

        private IEurexService eurexService;

        private IXetraService xetraService;

        private IRepository repository;

        private IAmazonSQS sqsClient;

        private ILambdaContext context;

        #endregion

        #region - Constructor -

        public ManagerService(
            IEurexService eurexService,
            IXetraService xetraService,
            IRepository repository,
            IAmazonSQS sqsClient,
            ILambdaContext context)
        {
            this.eurexService = eurexService;
            this.xetraService = xetraService;
            this.repository = repository;
            this.sqsClient = sqsClient;
            this.context = context;
        }

        #endregion

        #region - Public Methods | IManagerService -

        public async Task RoutePendingObjects(CancellationToken cancellationToken)
        {
            var startTime = this.context.RemainingTime;

            this.context.Logger.LogLine($"{this.context.RemainingTime.Subtract(startTime).ToString("hh':'mm':'ss")} - start enqueuing sqs messages for pending objects");

            await this.PushEurexPendingObjectMessages(cancellationToken);
            await this.PushXetraPendingObjectMessages(cancellationToken);

            this.context.Logger.LogLine($"{this.context.RemainingTime.Subtract(startTime).ToString("hh':'mm':'ss")} - finished enqueuing sqs messages for pending objects");
        }

        public async Task RouteUnQueuedObjects(CancellationToken cancellationToken)
        {
            await Task.FromException(new NotImplementedException());
        }

        #endregion

        #region - Private Methods -

        private async Task PushEurexPendingObjectMessages(CancellationToken cancellationToken)
        {
            var eurexPendingObjectKeyCollection = await this.DetectEurexPendingObjects(CumulusConstants.MaximumSQSMessagePushSize, cancellationToken);

            this.context.Logger.LogLine($"{eurexPendingObjectKeyCollection.Count} pending eurex object detected");

            if (eurexPendingObjectKeyCollection.Any())
                await this.PushSQSMessages(StockMarket.Eurex, eurexPendingObjectKeyCollection, cancellationToken);
        }

        private async Task PushXetraPendingObjectMessages(CancellationToken cancellationToken)
        {
            var xetraPendingObjectKeyCollection = await this.DetectXetraPendingObjects(CumulusConstants.MaximumSQSMessagePushSize, cancellationToken);

            this.context.Logger.LogLine($"{xetraPendingObjectKeyCollection.Count} pending xetra object detected");

            if (xetraPendingObjectKeyCollection.Any())
                await this.PushSQSMessages(StockMarket.Xetra, xetraPendingObjectKeyCollection, cancellationToken);
        }

        private async Task PushSQSMessages(StockMarket sourceBucketType, ICollection<BucketObject> bucketObjectCollection, CancellationToken cancellationToken)
        {
            var queueUrl = default(string);
            switch (sourceBucketType)
            {
                case StockMarket.Eurex:
                    queueUrl = CumulusConstants.MessageQueue.EurexPendingObjectQueueUrl;
                    break;
                case StockMarket.Xetra:
                    queueUrl = CumulusConstants.MessageQueue.XetraPendingObjectQueueUrl;
                    break;
                default:
                    throw new ArgumentException("Unknown source bucket type", nameof(sourceBucketType));
            }

            var sendMessageBatchRequest = new SendMessageBatchRequest()
            {
                QueueUrl = queueUrl
            };

            var messageDelayGap = CumulusConstants.MaximumSQSMessageDelaySeconds / bucketObjectCollection.Count;

            var messageDelay = 0;

            foreach (var bucketObject in bucketObjectCollection)
            {
                sendMessageBatchRequest.Entries.Add(new SendMessageBatchRequestEntry
                {
                    Id = bucketObject.ObjectID,
                    DelaySeconds = messageDelay,
                    MessageBody = bucketObject.ObjectKey
                });

                messageDelay += messageDelayGap;
            }

            var sendMessageBatchResponse = await this.sqsClient.SendMessageBatchAsync(sendMessageBatchRequest, cancellationToken);

            var queuedBucketObjectCollection = new List<BucketObject>();
            foreach (var sendMessageBatchResultEntry in sendMessageBatchResponse.Successful)
            {
                var bucketObject = bucketObjectCollection.Where(e => e.ObjectID == sendMessageBatchResultEntry.Id).FirstOrDefault();
                if (bucketObject != null)
                {
                    bucketObject.Status = (int)BucketObjectStatus.Queued;
                    bucketObject.CurrentStatusErrorCount = 0;
                    bucketObject.CurrentStatusLastError = null;

                    queuedBucketObjectCollection.Add(bucketObject);
                }
            }

            await this.repository.SaveItemBatchInternalAsync(queuedBucketObjectCollection, cancellationToken);

            var errorBucketObjectCollection = new List<BucketObject>();
            foreach (var batchResultErrorEntry in sendMessageBatchResponse.Failed)
            {
                var bucketObject = bucketObjectCollection.Where(e => e.ObjectID == batchResultErrorEntry.Id).FirstOrDefault();
                if (bucketObject != null)
                {
                    bucketObject.CurrentStatusErrorCount++;
                    bucketObject.CurrentStatusLastError = batchResultErrorEntry.Message;

                    errorBucketObjectCollection.Add(bucketObject);
                }
            }

            await this.repository.SaveItemBatchInternalAsync(errorBucketObjectCollection, cancellationToken);

            this.context.Logger.LogLine($"{sourceBucketType} - {queuedBucketObjectCollection.Count} sqs messages queued successfully and {errorBucketObjectCollection.Count} sqs messages were failed");
        }

        private async Task<List<BucketObject>> DetectEurexPendingObjects(int maxNumberOfPendingObjects, CancellationToken cancellationToken)
        {
            var pendingObjectCollection = await this.eurexService.GetPendingObjectKeyCollection(maxNumberOfPendingObjects, cancellationToken);

            await this.StorePendingObjects(pendingObjectCollection, cancellationToken);

            await this.eurexService.UpdateLastReadMarker(pendingObjectCollection.LastMarker, cancellationToken);

            return pendingObjectCollection;
        }

        private async Task<List<BucketObject>> DetectXetraPendingObjects(int maxNumberOfPendingObjects, CancellationToken cancellationToken)
        {
            var pendingObjectCollection = await this.xetraService.GetPendingObjectKeyCollection(maxNumberOfPendingObjects, cancellationToken);

            await this.StorePendingObjects(pendingObjectCollection, cancellationToken);

            await this.xetraService.UpdateLastReadMarker(pendingObjectCollection.LastMarker, cancellationToken);

            return pendingObjectCollection;
        }

        private async Task StorePendingObjects(PendingBucketObjectKeyCollection pendingObjectKeyCollection, CancellationToken cancellationToken)
        {
            foreach (var pendingObjectKey in pendingObjectKeyCollection)
            {
                pendingObjectKey.Status = (int)BucketObjectStatus.Detected;
                pendingObjectKey.CurrentStatusErrorCount = 0;
                pendingObjectKey.CurrentStatusLastError = null;
            }

            await this.repository.SaveItemBatchInternalAsync(pendingObjectKeyCollection, cancellationToken);
        }

        #endregion
    }
}