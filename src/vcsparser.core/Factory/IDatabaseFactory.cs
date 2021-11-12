using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.Documents;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core.Factory
{
    public interface IDatabaseFactory
    {
        ICosmosConnection CosmosConnection(string databaseId, int bulkBatchSize);
        IDocumentClient DocumentClient();
        IBulkExecutor BulkExecutor(IDocumentClient documentClient, DocumentCollection documentCollection);
        IBulkExecutorWrapper BulkExecutorWrapper(IBulkExecutor bulkExecutor);
    }
}
