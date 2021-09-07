using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace vcsparser.core.Database.Cosmos
{
    public class CosmosConnection : ICosmosConnection
    {
        private readonly string databaseId;
        private readonly IDocumentClient documentClient;
        private readonly IDatabaseFactory databaseFactory;

        public CosmosConnection(IDatabaseFactory databaseFactory, string databaseId)
        {
            this.databaseFactory = databaseFactory;
            this.documentClient = databaseFactory.DocumentClient();
            this.databaseId = databaseId;
        }

        public async Task<Document> CreateDocument(string collectionId, object document, RequestOptions options = null)
        {
            if (options == null)
                options = new RequestOptions { };

            var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            var response = await documentClient.CreateDocumentAsync(collectionUri, document, options);
            return response.Resource;
        }
    }
}
