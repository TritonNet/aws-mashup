using Cumulus.Aws.Common.BusinessModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Cumulus.Aws.Common.Abstraction
{
    public interface IProductService
    {
        Task SaveProductCollectionAsync(IEnumerable<Product> productCollection, CancellationToken cancellationToken);

        Task SaveTraceActivityCollectionAsync(IEnumerable<TradeActivity> tradeActivityCollection, CancellationToken cancellationToken);

        Task<List<Product>> GetProductsAsync(int limit, CancellationToken cancellationToken);

        Task<List<Product>> GetLastDayTrendingProductsAsync(int count, CancellationToken cancellationToken);

        Task<List<Product>> GetLastWeekTrendingProductsAsync(int count, CancellationToken cancellationToken);

        Task<List<Product>> GetMonthToDateTrendingProductsAsync(int count, CancellationToken cancellationToken);

        Task<List<Product>> GetYearToDateTrendingProductsAsync(int count, CancellationToken cancellationToken);

        Task<List<Product>> GetAllTimeTrendingProductsAsync(int count, CancellationToken cancellationToken);

        Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken);

        Task<List<TradeActivity>> GetTradeActivities(string productISIN, CancellationToken cancellationToken);
    }
}