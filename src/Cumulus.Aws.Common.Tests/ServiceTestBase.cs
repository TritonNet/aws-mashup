using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Cumulus.Aws.Common.Tests.MockServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cumulus.Aws.Common.Tests
{
    [TestClass]
    public abstract class ServiceTestBase
    {
        public async Task<ServiceContext> GetServiceTestContext(CancellationToken cancellationToken)
        {
            var serviceContext = new ServiceContext();
            var context = new TestLambdaContext();
            context.RemainingTime = TimeSpan.FromMinutes(5);

            await serviceContext.InjectServiceAsync<ILambdaContext>(context, cancellationToken);
            await serviceContext.InitializeAsync(cancellationToken);

            return serviceContext;
        }
    }
}
