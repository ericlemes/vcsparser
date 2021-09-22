using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core.Factory
{
    public class DatabaseFactory : IDatabaseFactory
    {
        private readonly string cosmosEndpoint;
        private readonly string cosmosDbKey;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public DatabaseFactory(GitExtractToCosmosDbCommandLineArgs args, JsonSerializerSettings jsonSerializerSettings)
            : this(args.CosmosDbKey, args.CosmosEndpoint, jsonSerializerSettings)
        {

        }

        public DatabaseFactory(DownloadFromCosmosDbCommandLineArgs args, JsonSerializerSettings jsonSerializerSettings)
            : this(args.CosmosDbKey, args.CosmosEndpoint, jsonSerializerSettings)
        {

        }

        private DatabaseFactory(string cosmosDbKey, string cosmosEndpoint,
            JsonSerializerSettings jsonSerializerSettings)
        {
            if (string.IsNullOrEmpty(cosmosDbKey))
                throw new ArgumentNullException(nameof(cosmosDbKey));
            if (string.IsNullOrEmpty(cosmosEndpoint))
                throw new ArgumentNullException(nameof(cosmosEndpoint));

            this.cosmosEndpoint = cosmosEndpoint;
            this.cosmosDbKey = cosmosDbKey;
            this.jsonSerializerSettings = jsonSerializerSettings;
        }

        public ICosmosConnection CosmosConnection(string databaseId)
        {
            return new CosmosConnection(this, databaseId);
        }

        public IDocumentClient DocumentClient()
        {
            var serviceEndPoint = new Uri(cosmosEndpoint);
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

            return new DocumentClient(serviceEndPoint, cosmosDbKey, jsonSerializerSettings, connectionPolicy, ConsistencyLevel.Session);
        }
    }
}
