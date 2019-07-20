
using Amazon.Lambda.SQSEvents;
using Amazon.Lambda.TestUtilities;
using System.Collections.Generic;
using Xunit;

namespace Cumulus.Aws.XetraMiddleware.Tests
{
    public class XetraHandlerTest
    {
        [Fact]
        public void TestToUpperFunction()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var handler = new XetraHandler();
            var context = new TestLambdaContext();

            var sqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>
                {
                    new SQSEvent.SQSMessage
                    {
                        Body = "2017-05-27/2017-05-27_BINS_XEUR06.csv"
                    }
                }
            };

            handler.Invoke(sqsEvent, context).Wait();
        }
    }
}
