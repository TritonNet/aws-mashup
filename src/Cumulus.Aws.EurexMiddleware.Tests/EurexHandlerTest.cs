using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using Cumulus.Aws.EurexMiddleware;
using Amazon.Lambda.SQSEvents;

namespace Cumulus.Aws.EurexMiddleware.Tests
{
    public class EurexHandlerTest
    {
        [Fact]
        public void TestToUpperFunction()
        {
            var handler = new EurexHandler();
            var context = new TestLambdaContext();
            context.RemainingTime = TimeSpan.FromMinutes(10);

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = "2017-05-29/2017-05-29_BINS_XEUR13.csv"
                    }
                }
            };

            handler.Invoke(sqsEvent, context).Wait();
        }
    }
}
