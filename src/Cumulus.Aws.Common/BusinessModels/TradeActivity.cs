using Amazon.DynamoDBv2.DataModel;
using Cumulus.Aws.Common.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cumulus.Aws.Common.BusinessModels
{
    [DynamoDBTable(CumulusConstants.TableName.TradeActivities)]
    public class TradeActivity
    {
        [DynamoDBHashKey(AttributeName = CumulusConstants.FieldName.ProductISIN), Column(nameof(ISIN))]
        public string ISIN { get; set; }

        [DynamoDBRangeKey(AttributeName = CumulusConstants.FieldName.TimeStamp), NotMapped]
        public double TimeStamp { get; set; }

        [DynamoDBIgnore, Column(nameof(Date))]
        public string Date { get; set; }

        [DynamoDBIgnore, Column(nameof(Time))]
        public string Time { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.StartPrice), Column(nameof(StartPrice))]
        public float StartPrice { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.MaxPrice), Column(nameof(MaxPrice))]
        public float MaxPrice { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.MinPrice), Column(nameof(MinPrice))]
        public float MinPrice { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.EndPrice), Column(nameof(EndPrice))]
        public float EndPrice { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.NumberOfTrades), Column(nameof(NumberOfTrades))]
        public int NumberOfTrades { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.PredictedEndPrice), NotMapped]
        public float PredictedEndPrice { get; set; }
    }
}
