using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cumulus.Aws.Common;
using Cumulus.Aws.Common.Infrastructure;
using Amazon.Lambda.Core;
using System.Threading;
using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.BusinessModels;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Cumulus.Aws.MaintenanceMiddleware
{
    public class MaintenanceHandler
    {
        #region - Private Properties -

        private ServiceContext serviceContext;

        #endregion

        public async Task Invoke(LambdaInvokeAction invokeAction, ILambdaContext context)
        {
            if (invokeAction == null)
                throw new ArgumentException("Unknown action");

            context.Logger.LogLine($"maintenance middleware invoked with action request '{invokeAction.Action}'");

            var cancellationTokenSource = new CancellationTokenSource(context.RemainingTime);

            var cancellationToken = cancellationTokenSource.Token;

            if (serviceContext == null)
            {
                serviceContext = new ServiceContext();
                await serviceContext.InjectServiceAsync(context, cancellationToken);
                await serviceContext.InitializeAsync(cancellationToken);
            }

            var service = await serviceContext.GetServiceAsync<IMaintenanceService>(cancellationToken);

            switch (invokeAction.Action)
            {
                case CumulusConstants.MaintenanceAction.EnsureTableCreated:
                    await service.EnsureAllTablesCreated(cancellationToken);
                    break;
                case CumulusConstants.MaintenanceAction.ImportProductEurex:
                    await service.ImportProduct(StockMarket.Eurex, cancellationToken);
                    break;
                case CumulusConstants.MaintenanceAction.ImportProductXetra:
                    await service.ImportProduct(StockMarket.Xetra, cancellationToken);
                    break;
                case CumulusConstants.MaintenanceAction.CalculateNumberOfTrades:
                    await service.CalculateNumberOfTrades(cancellationToken);
                    break;
                case CumulusConstants.MaintenanceAction.RerouteEurexDeadMessages:
                    await service.RerouteDeadMessages(StockMarket.Eurex, cancellationToken);
                    break;
                case CumulusConstants.MaintenanceAction.RerouteXetraDeadMessages:
                    await service.RerouteDeadMessages(StockMarket.Xetra, cancellationToken);
                    break;
                default: throw new ArgumentException("Unknown action");
            }

            context.Logger.LogLine($"maintenance middleware action '{invokeAction.Action}' completed successfully");
        }
    }
}
