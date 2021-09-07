using Microsoft.Azure.Documents;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace vcsparser.core.Database.Cosmos
{
    public interface ICosmosConnection
    {
        Task<Document> CreateDocument(string collectionId, object document, RequestOptions options = null);
    }
}
