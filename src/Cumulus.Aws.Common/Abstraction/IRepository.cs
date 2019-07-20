using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cumulus.Aws.Common.Abstraction
{
    internal interface IRepository
    {
        Task<Dictionary<string, bool>> GetTableAvailabilityStatusAsync(IEnumerable<string> tableNameCollection, CancellationToken cancellationToken);

        Task CreateTableInternalAsync<TTable>(CancellationToken cancellationToken);

        Task CreateTableInternalAsync(Type tableType, CancellationToken cancellationToken);

        Task SaveItemBatchInternalAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken);

        Task<T> GetItemAsync<T>(string key, CancellationToken cancellationToken);

        Task<List<T>> GetItemBatchAsync<T>(IEnumerable<string> keyCollection, CancellationToken cancellationToken);

        AsyncSearch<T> FromScanAsync<T>(ScanOperationConfig scanConfig, DynamoDBOperationConfig operationConfig = null);

        AsyncSearch<T> ScanAsync<T>(IEnumerable<ScanCondition> conditions, DynamoDBOperationConfig operationConfig = null);

        AsyncSearch<T> QueryAsync<T>(object hashKeyValue, DynamoDBOperationConfig operationConfig = null);

        Task SaveItemAsync<T>(T value, CancellationToken cancellationToken);

        Task<string> GetApplicationProperty(string property, CancellationToken cancellationToken);

        Task SetApplicationProperty(string property, string value, CancellationToken cancellationToken);
    }
}
