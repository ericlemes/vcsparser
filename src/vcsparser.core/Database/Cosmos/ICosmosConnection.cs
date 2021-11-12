using Microsoft.Azure.Documents;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;

namespace vcsparser.core.Database.Cosmos
{
    public interface ICosmosConnection
    {
        Task<Document> CreateDocument(string collectionId, object document, RequestOptions options = null);

        Task DeleteDocument(string collectionId, string documentId, RequestOptions options = null);

        IQueryable<T> CreateDocumentQuery<T>(string collectionId, SqlQuerySpec query, FeedOptions options = null);

        Task<CosmosBulkImportSummary> BulkInsertByBatchSize(string collectionId, IEnumerable<CosmosDocumentBase> documents, Action<CosmosBulkImportSummary> batchFinished = null);

        Task<BulkDeleteResponse> BulkDeleteDocuments(string collectionId, List<Tuple<string, string>> idsToDelete);
    }
}
