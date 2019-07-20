#region - References -
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.BusinessModels;
using Cumulus.Aws.Common.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Cumulus.Aws.Common.Implementation
{
    internal class MaintenanceService : IMaintenanceService
    {
        #region - Private Properties -

        private IRepository repository;

        private ILambdaContext context;

        private IAmazonS3 amazonS3;

        private IInstallService installService;

        private IProductService productService;

        private IAmazonSQS sqsClient;

        private IEncryptionService encryptionService;

        private IDateTimeService dateTimeService;

        #endregion

        #region - Constructor -

        public MaintenanceService(
            IRepository repository,
            ILambdaContext context,
            IAmazonS3 amazonS3,
            IInstallService installService,
            IProductService productService,
            IAmazonSQS sqsClient,
            IEncryptionService encryptionService,
            IDateTimeService dateTimeService)
        {
            this.repository = repository;
            this.context = context;
            this.amazonS3 = amazonS3;
            this.installService = installService;
            this.productService = productService;
            this.sqsClient = sqsClient;
            this.encryptionService = encryptionService;
            this.dateTimeService = dateTimeService;
        }

        #endregion

        #region - Public Methods | IMaintenanceService -

        public async Task EnsureAllTablesCreated(CancellationToken cancellationToken)
        {
            var tableNameTypeMappingCollection = new Dictionary<string, Type>
            {
                [CumulusConstants.TableName.ApplicationProperty] = typeof(ApplicationProperty),
                [CumulusConstants.TableName.BucketObjectStatus] = typeof(BucketObject),
                [CumulusConstants.TableName.Products] = typeof(Product),
                [CumulusConstants.TableName.TradeActivities] = typeof(TradeActivity)
            };

            var tableStatusCollection = await this.repository.GetTableAvailabilityStatusAsync(tableNameTypeMappingCollection.Keys, cancellationToken);

            foreach (var tableNameTypeMapping in tableNameTypeMappingCollection)
            {
                if (!tableStatusCollection[tableNameTypeMapping.Key])
                {
                    this.context.Logger.LogLine($"'{tableNameTypeMapping.Key}' not found and started creating");

                    await this.repository.CreateTableInternalAsync(tableNameTypeMapping.Value, cancellationToken);

                    this.context.Logger.LogLine($"'{tableNameTypeMapping.Key}' created");
                }
            }
        }

        public async Task ImportProduct(StockMarket stockMarket, CancellationToken cancellationToken)
        {
            var resourceBucketName = default(string);
            switch (stockMarket)
            {
                case StockMarket.Eurex:
                    resourceBucketName = CumulusConstants.SystemBucketObject.EurexProducts;
                    break;
                case StockMarket.Xetra:
                    resourceBucketName = CumulusConstants.SystemBucketObject.XetraProducts;
                    break;
                default: throw new ArgumentException("Invalid Stock Market");
            }

            var objectStream = await this.amazonS3.GetObjectStreamAsync(CumulusConstants.SystemBucket.Resource, resourceBucketName, null, cancellationToken);

            using (objectStream)
                await installService.InstallProductAsync(stockMarket, objectStream, cancellationToken);
        }

        public async Task CalculateNumberOfTrades(CancellationToken cancellationToken)
        {
            var productCollection = await productService.GetProductsAsync(cancellationToken);
            var take = 20;
            var @now = DateTime.UtcNow;

            var lastDayTimeStamp = this.dateTimeService.GetTimeStamp(new DateTime(@now.Year, @now.Month, @now.Day, 0, 0, 0, 0, DateTimeKind.Utc).AddDays(-1));
            var lastDayWeekTimeStamp = this.dateTimeService.GetTimeStamp(new DateTime(@now.Year, @now.Month, @now.Day, 0, 0, 0, 0, DateTimeKind.Utc).AddDays(-7));
            var mtdTimeStamp = this.dateTimeService.GetTimeStamp(new DateTime(@now.Year, @now.Month, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            var ytdTimeStamp = this.dateTimeService.GetTimeStamp(new DateTime(@now.Year, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            for (int pos = 0; pos < productCollection.Count; pos += take)
            {
                var productSubCollection = productCollection.Skip(pos).Take(take).ToList();

                foreach (var product in productSubCollection)
                {
                    try
                    {
                        var tradeActivityCollection = await productService.GetTradeActivities(product.ProductISIN, cancellationToken);

                        if (tradeActivityCollection.Any())
                        {
                            product.NumberOfTradesAll = tradeActivityCollection.Sum(e => e.NumberOfTrades);

                            var tradeActivityCollectionLastDay = tradeActivityCollection.Where(e => e.TimeStamp > lastDayTimeStamp);
                            if (tradeActivityCollectionLastDay.Any())
                                product.NumberOfTradesLastDay = tradeActivityCollectionLastDay.Sum(e => e.NumberOfTrades);

                            var tradeActivityCollectionLastWeek = tradeActivityCollection.Where(e => e.TimeStamp > lastDayWeekTimeStamp);
                            if (tradeActivityCollectionLastWeek.Any())
                                product.NumberOfTradesLastWeek = tradeActivityCollectionLastWeek.Sum(e => e.NumberOfTrades);

                            var tradeActivityCollectionMTD = tradeActivityCollection.Where(e => e.TimeStamp > mtdTimeStamp);
                            if (tradeActivityCollectionMTD.Any())
                                product.NumberOfTradesMonthToDate = tradeActivityCollectionMTD.Sum(e => e.NumberOfTrades);

                            var tradeActivityCollectionYTD = tradeActivityCollection.Where(e => e.TimeStamp > ytdTimeStamp);
                            if (tradeActivityCollectionYTD.Any())
                                product.NumberOfTradesYearToDate = tradeActivityCollectionYTD.Sum(e => e.NumberOfTrades);
                        }
                    }
                    catch (Exception exception)
                    {
                        this.context.Logger.LogLine(exception.Message);
                    }
                }

                await productService.SaveProductCollectionAsync(productSubCollection, cancellationToken);
            }
        }

        public async Task RerouteDeadMessages(StockMarket stockMarket, CancellationToken cancellationToken)
        {
            var deadMessageQueueUrl = default(string);
            var destinationMessageQueue = default(string);
            switch (stockMarket)
            {
                case StockMarket.Eurex:
                    deadMessageQueueUrl = CumulusConstants.MessageQueue.EurexDeadObjectQueueUrl;
                    destinationMessageQueue = CumulusConstants.MessageQueue.EurexPendingObjectQueueUrl;
                    break;
                case StockMarket.Xetra:
                    deadMessageQueueUrl = CumulusConstants.MessageQueue.XetraDeadObjectQueueUrl;
                    destinationMessageQueue = CumulusConstants.MessageQueue.XetraPendingObjectQueueUrl;
                    break;
                default:
                    break;
            }

            var deadMessageResponse = await this.sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                MaxNumberOfMessages = CumulusConstants.RerouteDeadMessageQueueCount,
                WaitTimeSeconds = CumulusConstants.RerouteDeadMessageQueueWaitTime,
                QueueUrl = deadMessageQueueUrl,
            });

            this.context.Logger.LogLine($"{deadMessageResponse.Messages.Count} dead messages retrived");

            var sendMessageBatchRequest = new SendMessageBatchRequest()
            {
                QueueUrl = destinationMessageQueue
            };

            var messageDelayGap = CumulusConstants.MaximumSQSMessageDelaySeconds / deadMessageResponse.Messages.Count;

            var messageDelay = 0;

            var newMessageIdCollection = deadMessageResponse
                                            .Messages
                                            .ToDictionary(
                                                deadMessage => Guid.NewGuid().ToStringID(),
                                                deadMessage => deadMessage
                                            );

            foreach (var deadmessage in deadMessageResponse.Messages)
            {
                var newMessageID = newMessageIdCollection
                                        .Where(e => e.Value.MessageId == deadmessage.MessageId)
                                        .Select(e => e.Key)
                                        .FirstOrDefault();

                if (!sendMessageBatchRequest.Entries.Where(e => e.MessageBody == deadmessage.Body).Any())
                {
                    sendMessageBatchRequest.Entries.Add(new SendMessageBatchRequestEntry
                    {
                        Id = newMessageID,
                        DelaySeconds = messageDelay,
                        MessageBody = deadmessage.Body
                    });

                    messageDelay += messageDelayGap;
                }
            }

            this.context.Logger.LogLine($"re-routing {sendMessageBatchRequest.Entries.Count} unique messages");

            var sendMessageBatchResponse = await this.sqsClient.SendMessageBatchAsync(sendMessageBatchRequest, cancellationToken);

            var deleteDeadMessageBatchRequest = new DeleteMessageBatchRequest
            {
                Entries = new List<DeleteMessageBatchRequestEntry>(),
                QueueUrl = deadMessageQueueUrl
            };

            this.context.Logger.LogLine($"{sendMessageBatchResponse.Successful.Count} messages re-routed successfully and {sendMessageBatchResponse.Failed.Count} messages failed");

            foreach (var reRouteSuccessfulMessage in sendMessageBatchResponse.Successful)
            {
                var deadMessage = newMessageIdCollection[reRouteSuccessfulMessage.Id];

                deleteDeadMessageBatchRequest.Entries.Add(new DeleteMessageBatchRequestEntry
                {
                    Id = deadMessage.MessageId,
                    ReceiptHandle = deadMessage.ReceiptHandle
                });
            }

            var deleteMessageBatchResponse = await this.sqsClient.DeleteMessageBatchAsync(deleteDeadMessageBatchRequest, cancellationToken);

            this.context.Logger.LogLine($"{deleteMessageBatchResponse.Successful.Count} messages successfully deleted from dead queue and {deleteMessageBatchResponse.Failed.Count} messages failed");
        }

        #endregion
    }
}