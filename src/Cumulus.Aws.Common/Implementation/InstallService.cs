#region - References -
using Amazon.Lambda.Core;
using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.BusinessModels;
using Cumulus.Aws.Common.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Cumulus.Aws.Common.Implementation
{
    internal class InstallService : IInstallService
    {
        #region - Private Properties -

        private ICsvConverter csvConverter;

        private IProductService cumulusDbContext;

        private ILambdaContext lambdaContext;

        #endregion

        #region - Constructor -

        public InstallService(
            ICsvConverter csvConverter,
            IProductService cumulusDbContext,
            ILambdaContext lambdaContext)
        {
            this.csvConverter = csvConverter;
            this.cumulusDbContext = cumulusDbContext;
            this.lambdaContext = lambdaContext;
        }

        #endregion

        #region - Public Methods | IInstallService -

        public async Task InstallProductAsync(StockMarket stockMarket, Stream stream, CancellationToken cancellationToken)
        {
            this.lambdaContext.Logger.LogLine($"{stockMarket} product import started");

            switch (stockMarket)
            {
                case StockMarket.Eurex:
                    await this.InstallProductEurexAsync(stream, cancellationToken);
                    break;
                case StockMarket.Xetra:
                    await this.InstallProductXetraAsync(stream, cancellationToken);
                    break;
                default: throw new ArgumentException("Invalid Stock Market");
            }

            this.lambdaContext.Logger.LogLine($"{stockMarket} product import completed");
        }

        #endregion

        #region - Private Methods -

        private async Task InstallProductEurexAsync(Stream stream, CancellationToken cancellationToken)
        {
            var enumerable = csvConverter.Convert<EurexProductSpecification>(stream, CumulusConstants.EurexProductCsvSeperator);

            var totalProductsImported = 0;

            using (var bae = new BufferedAsyncEnumerable<EurexProductSpecification>(enumerable, CumulusConstants.DynamoDbBatchWriteSize))
            {
                while (await bae.MoveNextAsync())
                {
                    var productCollection = bae.Current.Select(sourceProduct => new Product
                    {
                        ProductName = sourceProduct.ProductName,
                        ProductISIN = sourceProduct.ProductISIN,
                        StockMarket = (int)StockMarket.Eurex,
                        NumberOfTradesAll = 0,
                        NumberOfTradesLastDay = 0,
                        NumberOfTradesLastWeek = 0,
                        NumberOfTradesMonthToDate = 0,
                        NumberOfTradesYearToDate = 0,
                    })
                    .ToList();

                    await cumulusDbContext.SaveProductCollectionAsync(productCollection, cancellationToken);

                    totalProductsImported += productCollection.Count;
                }
            }

            this.lambdaContext.Logger.LogLine($"{totalProductsImported} products imported from eurex resource");
        }

        private async Task InstallProductXetraAsync(Stream stream, CancellationToken cancellationToken)
        {
            var enumerable = csvConverter.Convert<XetraProductSpecification>(stream, CumulusConstants.EurexProductCsvSeperator);

            var totalProductsImported = 0;

            using (var bae = new BufferedAsyncEnumerable<XetraProductSpecification>(enumerable, CumulusConstants.DynamoDbBatchWriteSize))
            {
                while (await bae.MoveNextAsync())
                {
                    var productCollection = bae.Current.Select(sourceProduct => new Product
                    {
                        ProductName = sourceProduct.Instrument,
                        ProductISIN = sourceProduct.ISIN,
                        StockMarket = (int)StockMarket.Xetra,
                        NumberOfTradesYearToDate = 0,
                        NumberOfTradesMonthToDate = 0,
                        NumberOfTradesLastWeek = 0,
                        NumberOfTradesLastDay = 0,
                        NumberOfTradesAll = 0
                    })
                    .ToList();

                    await cumulusDbContext.SaveProductCollectionAsync(productCollection, cancellationToken);

                    totalProductsImported += productCollection.Count;
                }
            }

            this.lambdaContext.Logger.LogLine($"{totalProductsImported} products imported from xetra resource");
        }

        #endregion
    }
}