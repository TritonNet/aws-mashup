#region - References -
using Amazon.Lambda.Core;
using Cumulus.Aws.Common;
using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.BusinessModels;
using Cumulus.Aws.Common.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

#region - Attributes -

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

#endregion

namespace Cumulus.Aws.LambdaManager
{
    public class Handler
    {
        #region - Private Properties -

        private ServiceContext serviceContext;

        #endregion

        #region - Public Methods -

        public async Task Invoke(LambdaInvokeAction invokeAction, ILambdaContext context)
        {
            if (invokeAction == null)
                throw new ArgumentException("Unknown action");

            context.Logger.LogLine($"lambda manager invoked with action request '{invokeAction.Action}'");

            var cancellationTokenSource = new CancellationTokenSource(context.RemainingTime);

            var cancellationToken = cancellationTokenSource.Token;

            if (serviceContext == null)
            {
                serviceContext = new ServiceContext();
                await serviceContext.InjectServiceAsync(context, cancellationToken);
                await serviceContext.InitializeAsync(cancellationToken);
            }

            var service = await serviceContext.GetServiceAsync<IManagerService>(cancellationToken);

            switch (invokeAction.Action)
            {
                case CumulusConstants.LambdaManagerAction.RoutePendingObjects:
                    await service.RoutePendingObjects(cancellationToken);
                    break;
                case CumulusConstants.LambdaManagerAction.RouteUnQueuedObjects:
                    await service.RouteUnQueuedObjects(cancellationToken);
                    break;
                default: throw new ArgumentException("Unknown action");
            }

            context.Logger.LogLine($"lambda manager action '{invokeAction.Action}' completed successfully");
        }

        #endregion
    }
}