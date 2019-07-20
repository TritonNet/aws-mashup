using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Cumulus.Aws.Common;
using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.BusinessModels;
using Cumulus.Aws.Common.Infrastructure;
using Cumulus.Aws.LambdaManager;
using Cumulus.Aws.MaintenanceMiddleware;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cumulus.Aws.Install
{
    class Program
    {
        static void Main(string[] args)
        {
            var function = new MaintenanceHandler();
            var context = new TestLambdaContext();
            context.RemainingTime = TimeSpan.FromHours(50);
            context.Logger = new ConsoleLogger();

            var invokeAction = new LambdaInvokeAction
            {
                Action = CumulusConstants.MaintenanceAction.CalculateNumberOfTrades,
            };

            function.Invoke(invokeAction, context).Wait();

            //var function = new Handler();
            //var context = new TestLambdaContext();
            //context.RemainingTime = TimeSpan.FromHours(50);
            //context.Logger = new ConsoleLogger();

            //var lambdaInvokeAction = new LambdaInvokeAction
            //{
            //    Action = CumulusConstants.LambdaManagerAction.RoutePendingObjects
            //};

            //function.Invoke(lambdaInvokeAction, context).Wait();
            Console.WriteLine("DONE !!!!!!!!!!!!");
            Console.ReadLine();
        }
    }

    public class ConsoleLogger : ILambdaLogger
    {
        public void Log(string message)
        {
            Console.Write(message);
        }

        public void LogLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}

