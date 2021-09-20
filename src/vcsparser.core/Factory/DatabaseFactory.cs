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
        private readonly string cosmosKey;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public DatabaseFactory(string cosmosEndpoint, string cosmosKey, JsonSerializerSettings jsonSerializerSettings)
        {
            this.cosmosEndpoint = cosmosEndpoint;
            this.cosmosKey = cosmosKey;
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

            return new DocumentClient(serviceEndPoint, this.cosmosKey, this.jsonSerializerSettings, connectionPolicy, ConsistencyLevel.Session);
        }
    }
}
