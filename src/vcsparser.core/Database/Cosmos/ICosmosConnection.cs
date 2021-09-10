using Microsoft.Azure.Documents;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using System.Linq;

namespace vcsparser.core.Database.Cosmos
{
    public interface ICosmosConnection
    {
        Task<Document> CreateDocument(string collectionId, object document, RequestOptions options = null);

        Task DeleteDocument(string collectionId, string documentId, RequestOptions options = null);

        IQueryable<T> CreateDocumentQuery<T>(string collectionId, SqlQuerySpec query, FeedOptions options = null);
    }
}
