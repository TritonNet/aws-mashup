using Amazon.DynamoDBv2.DataModel;
using Cumulus.Aws.Common.Infrastructure;

namespace Cumulus.Aws.Common.BusinessModels
{
    [DynamoDBTable(CumulusConstants.TableName.Products)]
    public class Product
    {
        [DynamoDBHashKey(AttributeName = CumulusConstants.FieldName.ProductISIN)]
        public string ProductISIN { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.StockMarket)]
        public int StockMarket { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.ProductName)]
        public string ProductName { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.NumberOfTradesLastDay)]
        public double NumberOfTradesLastDay { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.NumberOfTradesLastWeek)]
        public double NumberOfTradesLastWeek { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.NumberOfTradesMonthToDate)]
        public double NumberOfTradesMonthToDate { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.NumberOfTradesYearToDate)]
        public double NumberOfTradesYearToDate { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.NumberOfTradesAll)]
        public double NumberOfTradesAll { get; set; }

        [DynamoDBVersion(AttributeName = CumulusConstants.FieldName.VersionNumber)]
        public int? VersionNumber { get; set; }
    }
}