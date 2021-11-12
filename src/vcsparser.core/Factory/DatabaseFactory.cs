using System;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core.Factory
{
    public class DatabaseFactory : IDatabaseFactory
    {
        private readonly ICosmosCommandLineArgs cosmosCommandLineArgs;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public DatabaseFactory(ICosmosCommandLineArgs args, JsonSerializerSettings jsonSerializerSettings)
        {
            if (string.IsNullOrEmpty(args.CosmosDbKey))
                throw new ArgumentNullException(nameof(args.CosmosDbKey));
            if (string.IsNullOrEmpty(args.CosmosEndpoint))
                throw new ArgumentNullException(nameof(args.CosmosEndpoint));

            this.cosmosCommandLineArgs = args;
            this.jsonSerializerSettings = jsonSerializerSettings;
        }

        public IBulkExecutor BulkExecutor(IDocumentClient documentClient, DocumentCollection documentCollection)
        {
            return new BulkExecutor(documentClient as DocumentClient, documentCollection);
        }

        public IBulkExecutorWrapper BulkExecutorWrapper(IBulkExecutor bulkExecutor)
        {
            return new BulkExecutorWrapper(bulkExecutor as BulkExecutor);
        }

        public ICosmosConnection CosmosConnection(string databaseId, int bulkBatchSize)
        {
            return new CosmosConnection(this, databaseId, bulkBatchSize);
        }

        public IDocumentClient DocumentClient()
        {
            var serviceEndPoint = new Uri(cosmosCommandLineArgs.CosmosEndpoint);
            var connectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp,
                MaxConnectionLimit = 50,
                RetryOptions = new RetryOptions()
                {
                    MaxRetryAttemptsOnThrottledRequests = 9,
                    MaxRetryWaitTimeInSeconds = 30
                }
            };

            return new DocumentClient(serviceEndPoint, cosmosCommandLineArgs.CosmosDbKey, jsonSerializerSettings, connectionPolicy, ConsistencyLevel.Session);
        }
    }
}
