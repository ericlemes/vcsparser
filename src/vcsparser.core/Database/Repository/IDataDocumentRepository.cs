using System;
using System.Collections.Generic;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core.Database.Repository
{
    public interface IDataDocumentRepository
    {
        List<CosmosDataDocument<T>> GetDocumentsInDateRange<T>(string projectName, DocumentType documentType,
            DateTime fromDateTime,
            DateTime endDateTime) where T : IOutputJson;

        void CreateDataDocument<T>(CosmosDataDocument<T> document) where T : IOutputJson;

        int DeleteMultipleDocuments<T>(List<CosmosDataDocument<T>> documentsToDelete) where T : IOutputJson;

        CosmosBulkImportSummary BatchInsertCosmosDocuments<T>(
            IEnumerable<CosmosDataDocument<T>> cosmosDataDocuments,
            Action<CosmosBulkImportSummary> batchFinished = null) where T : IOutputJson;

        BulkDeleteResponse BatchDeleteDocuments(DateTime startDate, DateTime endDate, string projectName, DocumentType documentType);

        List<CosmosDataDocument<T>> GetAllDocumentsByProjectAndDocumentType<T>(string projectName,
            DocumentType documentType) where T : IOutputJson;
    }
}
