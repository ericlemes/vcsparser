using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using vcsparser.core.Database.Cosmos;
using Newtonsoft.Json;

namespace vcsparser.core.Database
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
            var serviceEndPoint = new Uri(this.cosmosEndpoint);
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
            var consistencyLevel = ConsistencyLevel.Session;
            return new DocumentClient(serviceEndPoint, this.cosmosKey, this.jsonSerializerSettings, connectionPolicy, consistencyLevel);
        }
    }
}
