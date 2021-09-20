using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using vcsparser.core.Factory;
using Xunit;

namespace vcsparser.unittests.Factory
{
    public class GivenADatabaseFactory
    {
        private readonly string cosmosEndpoint = "https://somedatabase:443";
        private readonly string cosmosDBKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private readonly DatabaseFactory sut;

        public GivenADatabaseFactory()
        {
            sut = new DatabaseFactory(cosmosEndpoint, cosmosDBKey, new JsonSerializerSettings());
        }


        [Fact]
        public void WhenCreatingDocumentClientShouldAllPropertiesShouldHaveCorrectValues()
        {
            var documentClient = sut.DocumentClient();
            Assert.Equal(ConnectionMode.Direct, documentClient.ConnectionPolicy.ConnectionMode);
            Assert.Equal(Protocol.Tcp, documentClient.ConnectionPolicy.ConnectionProtocol);
            Assert.Equal(50, documentClient.ConnectionPolicy.MaxConnectionLimit);
            Assert.Equal(30, documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds);
            Assert.Equal(9, documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests);
            Assert.Contains("https://somedatabase", documentClient.ServiceEndpoint.AbsoluteUri);
        }


        [Fact]
        public void WhenCosmosConnectionShouldNotThrowExceptions()
        {
            var result = Record.Exception(() =>
            {
                sut.CosmosConnection("some-id");
            });
            Assert.Null(result);
        }
    }
}
