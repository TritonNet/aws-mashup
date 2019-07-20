using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Cumulus.Aws.Common;
using Cumulus.Aws.Common.Abstraction;
using System.Threading;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Cumulus.Aws.EurexMiddleware
{
    public class EurexHandler
    {
        private static ServiceContext serviceContext;

        public async Task Invoke(SQSEvent evnt, ILambdaContext context)
        {
            context.Logger.LogLine($"{nameof(EurexHandler)} invoked with {evnt.Records.Count} sqs messages");

            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }

            context.Logger.LogLine($"{nameof(EurexHandler)} completed");
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"{nameof(EurexHandler)} started: object key : '{message.Body}'");

            var cancellationTokenSource = new CancellationTokenSource(context.RemainingTime);

            var cancellationToken = cancellationTokenSource.Token;

            if (serviceContext == null)
            {
                serviceContext = new ServiceContext();
                await serviceContext.InjectServiceAsync(context, cancellationToken);
                await serviceContext.InitializeAsync(cancellationToken);
            }

            var service = await serviceContext.GetServiceAsync<IEurexService>(cancellationToken);

            await service.ImportObjectAsync(message.Body, cancellationToken);

            context.Logger.LogLine($"{nameof(EurexHandler)} finished: object key : '{message.Body}'");
        }
    }
}
