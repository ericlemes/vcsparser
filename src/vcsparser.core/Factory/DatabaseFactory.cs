using System;
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

        public ICosmosConnection CosmosConnection(string databaseId)
        {
            return new CosmosConnection(this, databaseId);
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
