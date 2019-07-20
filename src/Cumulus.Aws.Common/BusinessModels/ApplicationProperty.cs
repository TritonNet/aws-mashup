using Amazon.DynamoDBv2.DataModel;
using Cumulus.Aws.Common.Infrastructure;

namespace Cumulus.Aws.Common.BusinessModels
{
    [DynamoDBTable(CumulusConstants.TableName.ApplicationProperty)]
    public class ApplicationProperty
    {
        [DynamoDBHashKey(AttributeName = CumulusConstants.FieldName.Property)]
        public string Property { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.Value)]
        public string Value { get; set; }
    }
}
