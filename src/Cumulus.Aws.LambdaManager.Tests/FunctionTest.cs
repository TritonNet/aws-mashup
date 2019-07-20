
using Amazon.Lambda.TestUtilities;
using Cumulus.Aws.Common.BusinessModels;
using Cumulus.Aws.Common.Infrastructure;
using System;
using Xunit;

namespace Cumulus.Aws.LambdaManager.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestToUpperFunction()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Handler();
            var context = new TestLambdaContext();
            context.RemainingTime = TimeSpan.FromMinutes(50);

            var lambdaInvokeAction = new LambdaInvokeAction
            {
                Action = CumulusConstants.LambdaManagerAction.RoutePendingObjects
            };

            function.Invoke(lambdaInvokeAction, context).Wait();
        }
    }
}
