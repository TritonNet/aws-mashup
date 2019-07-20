using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.BusinessModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cumulus.Aws.Api.Controllers
{
    [Route("api/products")]
    public class ProductsController : Controller
    {
        private IProductService productService;

        public ProductsController(
            IProductService productService)
        {
            this.productService = productService;
        }

        [HttpGet("all")]
        public async Task<List<Product>> GetAllProducts(CancellationToken cancellationToken)
        {
            var productCollection = await this.productService.GetAllTimeTrendingProductsAsync(100, cancellationToken);

            return productCollection.OrderByDescending(e => e.NumberOfTradesLastWeek).ToList();
        }

        [HttpGet("trending")]
        public async Task<List<Product>> GetTrendingProducts(CancellationToken cancellationToken)
        {
            //TODO : Move this 6 to a constant
            var productCollection = await this.productService.GetAllTimeTrendingProductsAsync(6, cancellationToken);

            return productCollection.OrderByDescending(e => e.NumberOfTradesLastWeek).ToList();
        }

        [HttpGet("{productISIN}/trades_start")]
        public async Task<List<double[]>> GetStartTradeActivities(string productISIN, CancellationToken cancellationToken)
        {
            var trades = await this.productService.GetTradeActivities(productISIN, cancellationToken);

            return trades.Select(e => new[] { (double)e.TimeStamp, (double)e.StartPrice }).ToList();
        }

        [HttpGet("{productISIN}/trades_min")]
        public async Task<List<double[]>> GetMinTradeActivities(string productISIN, CancellationToken cancellationToken)
        {
            var trades = await this.productService.GetTradeActivities(productISIN, cancellationToken);

            return trades.Select(e => new[] { (double)e.TimeStamp, (double)e.MinPrice }).ToList();
        }

        [HttpGet("{productISIN}/trades_max")]
        public async Task<List<double[]>> GetMaxTradeActivities(string productISIN, CancellationToken cancellationToken)
        {
            var trades = await this.productService.GetTradeActivities(productISIN, cancellationToken);

            return trades.Select(e => new[] { (double)e.TimeStamp, (double)e.MaxPrice }).ToList();
        }

        [HttpGet("{productISIN}/trades_end")]
        public async Task<List<double[]>> GetEndTradeActivities(string productISIN, CancellationToken cancellationToken)
        {
            var trades = await this.productService.GetTradeActivities(productISIN, cancellationToken);

            return trades.Select(e => new[] { (double)e.TimeStamp, (double)e.EndPrice }).ToList();
        }

        [HttpGet("{productISIN}/trades_num")]
        public async Task<List<double[]>> GetNumTradeActivities(string productISIN, CancellationToken cancellationToken)
        {
            var trades = await this.productService.GetTradeActivities(productISIN, cancellationToken);

            return trades.Select(e => new[] { (double)e.TimeStamp, (double)e.NumberOfTrades }).ToList();
        }

        [HttpGet("{productISIN}/trades_predicted")]
        public async Task<List<double[]>> GetPredictedTradeActivities(string productISIN, CancellationToken cancellationToken)
        {
            var trades = await this.productService.GetTradeActivities(productISIN, cancellationToken);

            return trades.Select(e => new[] { (double)e.TimeStamp, (double)e.PredictedEndPrice }).ToList();
        }
    }
}
