using Microsoft.Azure.Cosmos;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core.Factory
{
    public interface IDatabaseFactory
    {
        ICosmosConnection CosmosConnection(string databaseId, int bulkBatchSize);
        CosmosClient CosmosClient(CosmosClientOptions clientOptions = null);
        //IBulkExecutor BulkExecutor(IDocumentClient documentClient, DocumentCollection documentCollection);
        //IBulkExecutorWrapper BulkExecutorWrapper(IBulkExecutor bulkExecutor);
    }
}
