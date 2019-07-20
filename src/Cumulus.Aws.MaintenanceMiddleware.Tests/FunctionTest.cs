using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using Cumulus.Aws.MaintenanceMiddleware;
using Cumulus.Aws.Common.Infrastructure;
using Cumulus.Aws.Common.BusinessModels;

namespace Cumulus.Aws.MaintenanceMiddleware.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestToUpperFunction()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new MaintenanceHandler();
            var context = new TestLambdaContext();

            var invokeAction = new LambdaInvokeAction
            {
                Action = "",
            };

            function.Invoke(invokeAction, context).Wait();
        }

        [Fact]
        public void TestImportProductsEurex()
        {
            var function = new MaintenanceHandler();
            var context = new TestLambdaContext();
            context.RemainingTime = TimeSpan.FromMinutes(5);

            var invokeAction = new LambdaInvokeAction
            {
                Action = CumulusConstants.MaintenanceAction.ImportProductEurex,
            };

            function.Invoke(invokeAction, context).Wait();
        }

        [Fact]
        public void TestImportProductsXetra()
        {
            var function = new MaintenanceHandler();
            var context = new TestLambdaContext();
            context.RemainingTime = TimeSpan.FromMinutes(5);

            var invokeAction = new LambdaInvokeAction
            {
                Action = CumulusConstants.MaintenanceAction.ImportProductXetra,
            };

            function.Invoke(invokeAction, context).Wait();
        }

        [Fact]
        public void TestRerouteEurexDeadMessages()
        {
            var function = new MaintenanceHandler();
            var context = new TestLambdaContext();
            context.RemainingTime = TimeSpan.FromMinutes(5);

            var invokeAction = new LambdaInvokeAction
            {
                Action = CumulusConstants.MaintenanceAction.RerouteEurexDeadMessages,
            };

            function.Invoke(invokeAction, context).Wait();
        }
    }
}
