using System;
using System.Collections.Generic;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core.Database.Repository
{
    public interface IDataDocumentRepository
    {
        List<CosmosDataDocument<T>> GetDocumentsInDateRange<T>(DocumentType documentType, DateTime fromDateTime,
            DateTime endDateTime) where T : IOutputJson;

        void CreateDataDocument<T>(CosmosDataDocument<T> document) where T : IOutputJson;

        void DeleteMultipleDocuments<T>(List<CosmosDataDocument<T>> documentsToDelete) where T : IOutputJson;
    }
}
