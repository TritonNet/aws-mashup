namespace Cumulus.Aws.Common.Infrastructure
{
    public class CumulusConstants
    {
        public const string XetraBucketName = "deutsche-boerse-xetra-pds";

        public const string EurexBucketName = "deutsche-boerse-eurex-pds";

        public const char EurexProductCsvSeperator = ';';

        public const char EurexCsvSeperator = ',';

        public const char XetraCsvSeperator = ',';

        public const int DynamoDbBatchWriteSize = 25;

        public const int DynamoDbBatchGetSize = 100;

        public const int MaximumSQSMessageDelaySeconds = 900;

        public const int MaximumSQSMessagePushSize = 10;

        public const string CorsPolicyName = "CorsPolicy";

        public const int MaximumBucketObjectErrorThreshold = 3;

        public const int RerouteDeadMessageQueueCount = 10;

        public const int RerouteDeadMessageQueueWaitTime = 20;

        public class LambdaStatus
        {
            public const string Success = nameof(Success);
        }

        public class TableName
        {
            public const string Products = "products";
            public const string TradeActivities = "trade_activities";
            public const string BucketObjectStatus = "bucket_objects";
            public const string ApplicationProperty = "app_property";
        }

        public class FieldName
        {
            public const string ProductID = "product_id";
            public const string ProductType = "product_type";
            public const string ProductName = "product_name";
            public const string ProductGroup = "product_group";
            public const string Currency = "currency";
            public const string ProductISIN = "product_isin";
            public const string UnderlyingISIN = "underlying_isin";
            public const string ShareISIN = "share_isin";
            public const string CountryCode = "country_code";
            public const string CashMarketID = "cash_market_id";
            public const string StockMarket = "stock_market";

            public const string TimeStamp = "time_stamp";
            public const string StartPrice = "start_price";
            public const string MaxPrice = "max_price";
            public const string MinPrice = "min_price";
            public const string EndPrice = "end_price";
            public const string NumberOfTrades = "number_of_trades";
            public const string NumberOfTradesLastDay = "number_of_trades_lst_day";
            public const string NumberOfTradesLastWeek = "number_of_trades_lst_week";
            public const string NumberOfTradesMonthToDate = "number_of_trades_mtd";
            public const string NumberOfTradesYearToDate = "number_of_trades_ytd";
            public const string NumberOfTradesAll = "number_of_trades_all";
            public const string PredictedEndPrice = "predicted_end_price";

            public const string ObjectID = "object_id";
            public const string ObjectKey = "object_key";
            public const string Status = "status";
            public const string Size = "size";
            public const string SourceBucketType = "source_bucket_type";
            public const string CurrentStatusErrorCount = "current_status_error_count";
            public const string CurrentStatusLastError = "current_status_last_error";

            public const string Property = "property";
            public const string Value = "value";

            public const string VersionNumber = "version_number";
        }

        public class ApplicationProperty
        {
            public const string LastEurexObjectMarker = "last_eu_obj_marker";
            public const string LastXetraObjectMarker = "last_xe_obj_marker";
        }

        public class MessageQueue
        {
            // TODO : change this to read this values from environment variables
            public const string EurexPendingObjectQueueUrl = "https://sqs.eu-central-1.amazonaws.com/090188575979/eurex_pending_object_queue";
            public const string EurexDeadObjectQueueUrl = "https://sqs.eu-central-1.amazonaws.com/090188575979/eurex_dead_object_queue";
            public const string XetraPendingObjectQueueUrl = "https://sqs.eu-central-1.amazonaws.com/090188575979/xetra_pending_object_queue";
            public const string XetraDeadObjectQueueUrl = "https://sqs.eu-central-1.amazonaws.com/090188575979/xetra_dead_object_queue";
        }

        public class MessageQueueAttribute
        {
            public const string ApproximateNumberOfMessages = "ApproximateNumberOfMessages";
        }

        public class SecretManagerKey
        {
            public const string VstsReleaseManagerUserSecretKey = "user_vsts_release_manager_secretkey";
            public const string StockDataImporterUserSecretKey = "user_stock_data_importer_secretkey";
        }

        public class LambdaManagerAction
        {
            public const string RoutePendingObjects = "pending_objects";
            public const string RouteUnQueuedObjects = "unqueued_objects";
        }

        public class MaintenanceAction
        {
            public const string EnsureTableCreated = "ensure_table_created";
            public const string ImportProductEurex = "import_product_eurex";
            public const string ImportProductXetra = "import_product_xetra";
            public const string CalculateNumberOfTrades = "calc_num_of_trades";
            public const string RerouteEurexDeadMessages = "reroute_eurex_dead_msgs";
            public const string RerouteXetraDeadMessages = "reroute_xetra_dead_msgs";
        }

        public class JsonField
        {
            public const string Action = "action";
        }

        public class SystemBucket
        {
            public const string Resource = "sysresources";
        }

        public class SystemBucketObject
        {
            public const string EurexProducts = "eurex.productlist.csv";
            public const string XetraProducts = "xetra.productlist.csv";
        }
    }
}
