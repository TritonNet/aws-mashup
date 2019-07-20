using Cumulus.Aws.Common.BusinessModels;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cumulus.Aws.Common.Abstraction
{
    public interface IInstallService
    {
        Task InstallProductAsync(StockMarket stockMarket, Stream stream, CancellationToken cancellationToken);
    }
}
