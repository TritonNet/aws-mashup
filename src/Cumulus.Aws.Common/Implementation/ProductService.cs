#region - References -
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
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
    internal class ProductService : IProductService
    {
        #region - Private Properties -

        private IRepository repository;

        private IEncryptionService encryptionService;

        private IDateTimeService dateTimeService;

        private ILambdaContext context;

        #endregion

        #region - Constructor -

        public ProductService(
            IRepository repository,
            IEncryptionService encryptionService,
            IDateTimeService dateTimeService,
            ILambdaContext context)
        {
            this.repository = repository;
            this.encryptionService = encryptionService;
            this.dateTimeService = dateTimeService;
            this.context = context;
        }

        #endregion

        #region - Public Methods | IProductService -

        public async Task SaveProductCollectionAsync(IEnumerable<Product> productCollection, CancellationToken cancellationToken)
        {
            try
            {
                await this.repository.SaveItemBatchInternalAsync(productCollection, cancellationToken);
            }
            catch (ResourceNotFoundException)
            {
                await this.repository.CreateTableInternalAsync<Product>(cancellationToken);
                await this.repository.SaveItemBatchInternalAsync(productCollection, cancellationToken);
            }
        }

        public async Task SaveTraceActivityCollectionAsync(IEnumerable<TradeActivity> tradeActivityCollection, CancellationToken cancellationToken)
        {
            foreach (var tradeActivity in tradeActivityCollection)
            {
                if (!DateTime.TryParse($"{tradeActivity.Date} {tradeActivity.Time}", out var dateTime))
                    throw new InvalidCastException($"Invalid 'Date' ('{tradeActivity.Date}') or 'Time' ('{tradeActivity.Time}') value in {nameof(tradeActivity.ISIN)} = '{tradeActivity.ISIN}'");

                tradeActivity.TimeStamp = this.dateTimeService.GetTimeStamp(dateTime);
            }

            try
            {
                await this.repository.SaveItemBatchInternalAsync(tradeActivityCollection, cancellationToken);
            }
            catch (ResourceNotFoundException)
            {
                await this.repository.CreateTableInternalAsync<TradeActivity>(cancellationToken);
                await this.repository.SaveItemBatchInternalAsync(tradeActivityCollection, cancellationToken);
            }
        }

        public async Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken)
        {
            var scanResult = this.repository.ScanAsync<Product>(Enumerable.Empty<ScanCondition>());

            return await this.GetAsyncSearchResult(scanResult, cancellationToken);
        }

        public async Task<List<Product>> GetProductsAsync(int limit, CancellationToken cancellationToken)
        {
            var scanOperation = new ScanOperationConfig
            {
                Limit = limit,
            };

            var scanResult = this.repository.FromScanAsync<Product>(scanOperation);

            var productCollection = await this.GetAsyncSearchResult(scanResult, cancellationToken);

            return productCollection.Take(limit).ToList();
        }

        public async Task<List<Product>> GetAllTimeTrendingProductsAsync(int count, CancellationToken cancellationToken)
        {
            var productCollection = await this.GetTrendingProductsAsync(CumulusConstants.FieldName.NumberOfTradesAll, cancellationToken);

            return productCollection.OrderByDescending(e => e.NumberOfTradesAll).Take(count).ToList();
        }

        public async Task<List<Product>> GetLastDayTrendingProductsAsync(int count, CancellationToken cancellationToken)
        {
            var productCollection = await this.GetTrendingProductsAsync(CumulusConstants.FieldName.NumberOfTradesLastDay, cancellationToken);

            return productCollection.OrderByDescending(e => e.NumberOfTradesLastDay).Take(count).ToList();
        }

        public async Task<List<Product>> GetLastWeekTrendingProductsAsync(int count, CancellationToken cancellationToken)
        {
            var productCollection = await this.GetTrendingProductsAsync(CumulusConstants.FieldName.NumberOfTradesLastWeek, cancellationToken);

            return productCollection.OrderByDescending(e => e.NumberOfTradesLastWeek).Take(count).ToList();
        }

        public async Task<List<Product>> GetMonthToDateTrendingProductsAsync(int count, CancellationToken cancellationToken)
        {
            var productCollection = await this.GetTrendingProductsAsync(CumulusConstants.FieldName.NumberOfTradesMonthToDate, cancellationToken);

            return productCollection.OrderByDescending(e => e.NumberOfTradesMonthToDate).Take(count).ToList();
        }

        public async Task<List<Product>> GetYearToDateTrendingProductsAsync(int count, CancellationToken cancellationToken)
        {
            var productCollection = await this.GetTrendingProductsAsync(CumulusConstants.FieldName.NumberOfTradesYearToDate, cancellationToken);

            return productCollection.OrderByDescending(e => e.NumberOfTradesYearToDate).Take(count).ToList();
        }

        public async Task<List<TradeActivity>> GetTradeActivities(string productISIN, CancellationToken cancellationToken)
        {
            var queryResult = this.repository.QueryAsync<TradeActivity>(productISIN);

            var trades = await queryResult.GetNextSetAsync(cancellationToken);

            while (!queryResult.IsDone && !cancellationToken.IsCancellationRequested)
                trades.AddRange(await queryResult.GetRemainingAsync(cancellationToken));

            return trades;
        }

        #endregion

        #region - Private Methods -

        private async Task<List<Product>> GetTrendingProductsAsync(string attributeName, CancellationToken cancellationToken)
        {
            var scanOperation = new ScanOperationConfig
            {
                Filter = new ScanFilter()
            };

            var scn = new Condition
            {
                AttributeValueList = new List<AttributeValue>
                {
                    new AttributeValue
                    {
                        N = "0",
                    }
                },
                ComparisonOperator = ComparisonOperator.GT
            };

            scanOperation.Filter.AddCondition(attributeName, scn);

            var scanResult = this.repository.FromScanAsync<Product>(scanOperation);

            return await this.GetAsyncSearchResult(scanResult, cancellationToken);
        }

        public async Task<List<T>> GetAsyncSearchResult<T>(AsyncSearch<T> asyncSearch, CancellationToken cancellationToken)
        {
            var results = await asyncSearch.GetNextSetAsync(cancellationToken);

            while (!asyncSearch.IsDone && !cancellationToken.IsCancellationRequested)
                results.AddRange(await asyncSearch.GetRemainingAsync(cancellationToken));

            return results;
        }

        #endregion
    }
}