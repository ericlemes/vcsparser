using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using vcsparser.core.Factory;

namespace vcsparser.core.Database.Cosmos
{
    public class CosmosConnection : ICosmosConnection
    {
        private readonly string databaseId;
        private readonly int bulkBatchSize;
        
        private readonly CosmosClient client;
        private readonly Microsoft.Azure.Cosmos.Database database;
        private readonly IDatabaseFactory databaseFactory;

        public CosmosConnection(IDatabaseFactory databaseFactory, string databaseId, int bulkBatchSize)
        {
            this.databaseFactory = databaseFactory;
            this.databaseId = databaseId;
            this.bulkBatchSize = bulkBatchSize;

            this.client = databaseFactory.CosmosClient(new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct
            });
            this.database = this.client.GetDatabase(databaseId);
        }

        [ExcludeFromCodeCoverage]
        //This method is ignored because OpenCover can't cover all branches for async methods
        public async Task<T> CreateItem<T>(string containerId, T item) where T : CosmosDocumentBase
        {
            return await CreateItem<T>(containerId, item.Id!, item);               
        }

        public async Task<T> CreateItem<T>(string containerId, string partitionKey, T item) where T : CosmosDocumentBase
        {
            //Forces a serialization/deserialization before adding, so attributes like "Required" will be validated.
            client.ClientOptions.Serializer.FromStream<T>(client.ClientOptions.Serializer.ToStream(item));

            var container = database.GetContainer(containerId);
            var key = new PartitionKey(partitionKey);

            var response = await container.CreateItemAsync<T>(item, key);
            return response.Resource;
        }

        public async Task DeleteItem<T>(string containerId, string partitionKey, T item) where T : CosmosDocumentBase
        {
            var container = database.GetContainer(containerId);
            var key = new PartitionKey(partitionKey);
            var options = new ItemRequestOptions
            {
                IfMatchEtag = item.ETag
            };

            await container.DeleteItemAsync<T>(item.Id, key, options);
        }

        public async Task DeleteItems<T>(string containerId, string partitionKey, IEnumerable<T> items) where T : CosmosDocumentBase
        {
            if (!items.Any())
                return;

            var container = database.GetContainer(containerId);
            var key = new PartitionKey(partitionKey);
            var batch = container.CreateTransactionalBatch(key);

            foreach (var item in items)
            {
                batch.DeleteItem(item.Id, new TransactionalBatchItemRequestOptions
                {
                    IfMatchEtag = item.ETag
                });
            }
            var response = await batch.ExecuteAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception("Error in batch delete");
        }

        private void UpdateCosmosBulkImportSummary(CosmosBulkImportSummary jobSummray, CosmosBulkImportSummary batchSummray)
        {
            jobSummray.NumberOfDocumentsInserted += batchSummray.NumberOfDocumentsInserted;
            jobSummray.TotalRequestUnitsConsumed += batchSummray.TotalRequestUnitsConsumed;
            jobSummray.TotalTimeTaken += batchSummray.TotalTimeTaken;
            jobSummray.NumberOfBatchesCompleted++;

            batchSummray.NumberOfBatchesCompleted = jobSummray.NumberOfBatchesCompleted;
            batchSummray.NumberOfBatches = jobSummray.NumberOfBatches;
        }

        public async Task<List<T>> QueryItems<T>(string containerId, QueryDefinition queryDefinition) where T : CosmosDocumentBase
        {
            var container = database.GetContainer(containerId);

            var resultSet = container.GetItemQueryIterator<T>(queryDefinition);
            var results = new List<T>();
            while (resultSet.HasMoreResults)
            {
                var response = await resultSet.ReadNextAsync();
                results.AddRange(response.Resource);
            }
            return results;
        }
    }
}
