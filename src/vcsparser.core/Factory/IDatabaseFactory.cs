using Microsoft.Azure.Documents;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core.Factory
{
    public interface IDatabaseFactory
    {
        ICosmosConnection CosmosConnection(string databaseId);
        IDocumentClient DocumentClient();
    }
}
