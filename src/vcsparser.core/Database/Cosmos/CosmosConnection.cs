using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace vcsparser.core.Database.Cosmos
{
    public class CosmosConnection : ICosmosConnection
    {
        private readonly string databaseId;
        private readonly IDocumentClient documentClient;

        public CosmosConnection(IDatabaseFactory databaseFactory, string databaseId)
        {
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

        public async Task DeleteDocument(string collectionId, string documentId, RequestOptions options = null)
        {
            if (options == null)
                options = new RequestOptions
                {
                    PartitionKey = new PartitionKey(documentId)
                };

            var documentUri = UriFactory.CreateDocumentUri(databaseId, collectionId, documentId);
            await documentClient.DeleteDocumentAsync(documentUri, options);
        }


        public IQueryable<T> CreateDocumentQuery<T>(string collectionId, SqlQuerySpec query, FeedOptions options = null)
        {
            if (options == null)
                options = new FeedOptions
                {
                    MaxItemCount = -1,
                    EnableCrossPartitionQuery = true
                };

            var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            return documentClient.CreateDocumentQuery<T>(collectionUri, query, options);
        }
    }
}
