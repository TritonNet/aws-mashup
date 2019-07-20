using Cumulus.Aws.Common.BusinessModels;
using System.Threading;
using System.Threading.Tasks;

namespace Cumulus.Aws.Common.Abstraction
{
    public interface IMaintenanceService
    {
        Task EnsureAllTablesCreated(CancellationToken cancellationToken);

        Task ImportProduct(StockMarket stockMarket, CancellationToken cancellationToken);

        Task CalculateNumberOfTrades(CancellationToken cancellationToken);

        Task RerouteDeadMessages(StockMarket stockMarket, CancellationToken cancellationToken);
    }
}
