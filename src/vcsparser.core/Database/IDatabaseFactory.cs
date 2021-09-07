using Microsoft.Azure.Documents;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core.Database
{
    public interface IDatabaseFactory
    {
        ICosmosConnection CosmosConnection(string databaseId);
        IDocumentClient DocumentClient();
    }
}
