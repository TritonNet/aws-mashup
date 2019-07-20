using Amazon.DynamoDBv2;
using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.Tests.MockServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Cumulus.Aws.Common.Tests
{
    [TestClass]
    public class ProductServiceTests : ServiceTestBase
    {
        [TestMethod]
        public void TestGetTopNProducts()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var serviceContext = this.GetServiceTestContext(cancellationTokenSource.Token).Result;

            var service = serviceContext.GetServiceAsync<IProductService>(cancellationTokenSource.Token).Result;

            var products = service.GetProductsAsync(5, CancellationToken.None).Result;

            Assert.IsNotNull(products);
        }

        [TestMethod]
        public void TestGetAllProducts()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var serviceContext = this.GetServiceTestContext(cancellationTokenSource.Token).Result;

            var service = serviceContext.GetServiceAsync<IProductService>(cancellationTokenSource.Token).Result;

            var products = service.GetProductsAsync(CancellationToken.None).Result;

            Assert.IsNotNull(products);
        }

        [TestMethod]
        public void TestGetLastWeekTrending()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var serviceContext = this.GetServiceTestContext(cancellationTokenSource.Token).Result;

            var service = serviceContext.GetServiceAsync<IProductService>(cancellationTokenSource.Token).Result;

            var products = service.GetAllTimeTrendingProductsAsync(100, CancellationToken.None).Result;

            Assert.IsNotNull(products);
        }
    }
}
