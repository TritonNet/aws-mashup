#region - References -
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.BusinessModels;
using Cumulus.Aws.Common.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Cumulus.Aws.Common.Implementation
{
    internal class Repository : DynamoDBContext, IRepository
    {
        #region - Private Properties -

        private IAmazonDynamoDB dynamoDB;

        #endregion

        #region - Constructor -

        public Repository(IAmazonDynamoDB dynamoDB)
            : base(dynamoDB)
        {
            this.dynamoDB = dynamoDB;
        }

        #endregion

        #region - Public Methods | IRepository -

        public async Task CreateTableInternalAsync<TTable>(CancellationToken cancellationToken)
        {
            await this.CreateTableInternalAsync(typeof(TTable), cancellationToken);
        }

        public async Task CreateTableInternalAsync(Type tableType, CancellationToken cancellationToken)
        {
            var createTableRequest = new CreateTableRequest();
            createTableRequest.TableName = this.GetTableName(tableType);
            createTableRequest.ProvisionedThroughput = new ProvisionedThroughput
            {
                // TODO : Find what those values are
                ReadCapacityUnits = 2,
                WriteCapacityUnits = 2
            };

            createTableRequest.KeySchema = new List<KeySchemaElement>();
            createTableRequest.AttributeDefinitions = new List<AttributeDefinition>();

            foreach (var property in tableType.GetProperties())
            {
                var propertyAttribute = default(DynamoDBPropertyAttribute);

                var dynamoDBHashKey = (DynamoDBHashKeyAttribute)Attribute.GetCustomAttribute(property, typeof(DynamoDBHashKeyAttribute));
                if (dynamoDBHashKey != null)
                {
                    propertyAttribute = dynamoDBHashKey;
                    createTableRequest.KeySchema.Add(new KeySchemaElement
                    {
                        AttributeName = dynamoDBHashKey.AttributeName,
                        KeyType = "HASH",
                    });
                }

                var rangeKeyAttribute = (DynamoDBRangeKeyAttribute)Attribute.GetCustomAttribute(property, typeof(DynamoDBRangeKeyAttribute));
                if (rangeKeyAttribute != null)
                {
                    propertyAttribute = rangeKeyAttribute;
                    createTableRequest.KeySchema.Add(new KeySchemaElement
                    {
                        AttributeName = rangeKeyAttribute.AttributeName,
                        KeyType = "RANGE",
                    });
                }

                if (propertyAttribute != null)
                {
                    var attributeDefinition = new AttributeDefinition();
                    attributeDefinition.AttributeName = propertyAttribute.AttributeName ?? property.Name;

                    if (property.PropertyType.IsNumericType())
                        attributeDefinition.AttributeType = "N";
                    else if (property.PropertyType == typeof(string))
                        attributeDefinition.AttributeType = "S";
                    else
                        throw new NotSupportedException("Unknwon Type");

                    createTableRequest.AttributeDefinitions.Add(attributeDefinition);
                }
            }

            await this.dynamoDB.CreateTableAsync(createTableRequest, cancellationToken);
            await this.WaitForTableActive(createTableRequest.TableName, cancellationToken);
        }

        public async Task<Dictionary<string, bool>> GetTableAvailabilityStatusAsync(IEnumerable<string> tableNameCollection, CancellationToken cancellationToken)
        {
            var listTableResponse = await this.dynamoDB.ListTablesAsync(cancellationToken);

            return tableNameCollection
                    .ToDictionary(
                        tableName => tableName,
                        tableName => listTableResponse.TableNames.Contains(tableName)
                    );
        }

        public async Task SaveItemBatchInternalAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken)
        {
            var operationConfig = new DynamoDBOperationConfig
            {
                SkipVersionCheck = true // Batch write operation doesnt support automatic version checking
            };

            var itemBatch = this.CreateBatchWrite<T>(operationConfig);

            itemBatch.AddPutItems(items);

            await itemBatch.ExecuteAsync(cancellationToken);
        }

        public async Task<T> GetItemAsync<T>(string key, CancellationToken cancellationToken)
        {
            try
            {
                return await this.LoadAsync<T>(key, cancellationToken);
            }
            catch (ResourceNotFoundException)
            {
                return default(T);
            }
        }

        public async Task SaveItemAsync<T>(T value, CancellationToken cancellationToken)
        {
            try
            {
                await this.SaveAsync(value, cancellationToken);
            }
            catch (ResourceNotFoundException)
            {
                await this.CreateTableInternalAsync<T>(cancellationToken);
                await this.SaveAsync(value, cancellationToken);
            }
        }

        public async Task<List<T>> GetItemBatchAsync<T>(IEnumerable<string> keyCollection, CancellationToken cancellationToken)
        {
            try
            {
                return await this.GetItemBatchInternalAsync<T>(keyCollection, cancellationToken);
            }
            catch (ResourceNotFoundException)
            {
                await this.CreateTableInternalAsync<T>(cancellationToken);
                return await this.GetItemBatchInternalAsync<T>(keyCollection, cancellationToken);
            }
        }

        public async Task<string> GetApplicationProperty(string property, CancellationToken cancellationToken)
        {
            var applicationProperty = await this.GetItemAsync<ApplicationProperty>(property, cancellationToken);

            return applicationProperty?.Value;
        }

        public async Task SetApplicationProperty(string property, string value, CancellationToken cancellationToken)
        {
            var applicationProperty = await this.GetItemAsync<ApplicationProperty>(property, cancellationToken);

            if (applicationProperty == null)
            {
                applicationProperty = new ApplicationProperty
                {
                    Property = property,
                };
            }

            applicationProperty.Value = value;

            await this.SaveItemAsync(applicationProperty, cancellationToken);
        }

        #endregion

        #region - Private Methods -

        private string GetTableName<T>()
        {
            return this.GetTableName(typeof(T));
        }

        private string GetTableName(Type tableType)
        {
            var dynamoDBTableAttribute = (DynamoDBTableAttribute)Attribute.GetCustomAttribute(tableType, typeof(DynamoDBTableAttribute));
            if (dynamoDBTableAttribute == null)
                throw new ArgumentNullException(nameof(DynamoDBTableAttribute));

            return dynamoDBTableAttribute.TableName;
        }

        private async Task<List<T>> GetItemBatchInternalAsync<T>(IEnumerable<string> keyCollection, CancellationToken cancellationToken)
        {
            var batchGet = this.CreateBatchGet<T>();

            foreach (var key in keyCollection)
                batchGet.AddKey(key);

            await batchGet.ExecuteAsync(cancellationToken);

            return batchGet.Results;
        }

        private async Task WaitForTableActive(string tableName, CancellationToken cancellationToken)
        {
            var status = TableStatus.CREATING;
            do
            {
                await Task.Delay(5000, cancellationToken); // Wait 5 seconds.
                try
                {
                    var request = new DescribeTableRequest(tableName);

                    var response = await this.dynamoDB.DescribeTableAsync(request, cancellationToken);

                    Debug.WriteLine($"Table name: {response.Table.TableName}, status: {response.Table.TableStatus}");

                    status = response.Table.TableStatus;
                }
                catch (ResourceNotFoundException)
                {
                    // if the table has not created in 5secs, we get ResourceNotFoundException. swallow it and try again
                }

            } while (status != TableStatus.ACTIVE && !cancellationToken.IsCancellationRequested);
        }

        #endregion
    }
}