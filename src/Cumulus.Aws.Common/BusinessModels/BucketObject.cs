using Amazon.DynamoDBv2.DataModel;
using Cumulus.Aws.Common.Infrastructure;

namespace Cumulus.Aws.Common.BusinessModels
{
    [DynamoDBTable(CumulusConstants.TableName.BucketObjectStatus)]
    public class BucketObject
    {
        [DynamoDBHashKey(AttributeName = CumulusConstants.FieldName.ObjectID)]
        public string ObjectID { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.ObjectKey)]
        public string ObjectKey { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.SourceBucketType)]
        public int SourceBucketType { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.Status)]
        public int Status { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.Size)]
        public long ObjectSize { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.CurrentStatusErrorCount)]
        public int CurrentStatusErrorCount { get; set; }

        [DynamoDBProperty(AttributeName = CumulusConstants.FieldName.CurrentStatusLastError)]
        public string CurrentStatusLastError { get; set; }

        [DynamoDBVersion(AttributeName = CumulusConstants.FieldName.VersionNumber)]
        public int? VersionNumber { get; set; }
    }
}
