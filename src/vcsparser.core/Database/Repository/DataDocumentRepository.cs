using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Microsoft.Azure.Documents;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core.Database.Repository
{
    public class DataDocumentRepository : IDataDocumentRepository
    {
        private readonly string cosmosDbContainer;
        private readonly ICosmosConnection cosmosConnection;

        public DataDocumentRepository(ICosmosConnection cosmosConnection, string cosmosDbContainer)
        {
            this.cosmosDbContainer = cosmosDbContainer;
            this.cosmosConnection = cosmosConnection;
        }
        public void CreateDataDocument<T>(CosmosDataDocument<T> document) where T : IOutputJson
        {
            cosmosConnection.CreateDocument(cosmosDbContainer, document).Wait();
        }

        public List<CosmosDataDocument<T>> GetDocumentsInDateRange<T>(string projectName, DocumentType documentType, DateTime fromDateTime, DateTime endDateTime) where T : IOutputJson
        {
            var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.projectName = '{projectName}' and c.documentType = '{documentType}' and (c.occurrenceDate between '{ fromDateTime.ToString(CosmosDataDocument<T>.DATE_FORMAT) }' and '{ endDateTime.ToString(CosmosDataDocument<T>.DATE_FORMAT) }') order by c.occurrenceDate desc");
            return cosmosConnection.CreateDocumentQuery<CosmosDataDocument<T>>(cosmosDbContainer, sqlQuery).ToList();
        }

        public List<CosmosDataDocument<T>> GetAllDocumentsByProjectAndDocumentType<T>(string projectName, DocumentType documentType) where T : IOutputJson
        {
            var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.projectName = '{projectName}' and c.documentType = '{documentType}' order by c.occurrenceDate desc");
            return cosmosConnection.CreateDocumentQuery<CosmosDataDocument<T>>(cosmosDbContainer, sqlQuery).ToList();
        }

        public int DeleteMultipleDocuments<T>(List<CosmosDataDocument<T>> documentsToDelete) where T : IOutputJson
        {
            var deletedDocuments = 0;
            foreach (var documentToDelete in documentsToDelete)
            {
                var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.documentType= '{documentToDelete.DocumentType}' and c.projectName = '{documentToDelete.ProjectName}' and c.occurrenceDate = '{documentToDelete.DateTime:yyyy-MM-ddTHH:mm:ss}'");

                var result = cosmosConnection.CreateDocumentQuery<CosmosDataDocument<T>>(cosmosDbContainer, sqlQuery, null).ToList();if (result.Count <= 0) continue;

                foreach (var cosmosDataDocuments in result)
                {
                    deletedDocuments++;
                    cosmosConnection.DeleteDocument(cosmosDbContainer, cosmosDataDocuments.Id).Wait();
                }
            }

            return deletedDocuments;
        }

        public CosmosBulkImportSummary BatchInsertCosmosDocuments<T>(IEnumerable<CosmosDataDocument<T>> cosmosDataDocuments, Action<CosmosBulkImportSummary> batchFinished = null) where T : IOutputJson
        {
            return cosmosConnection.BulkInsertByBatchSize(cosmosDbContainer, cosmosDataDocuments, batchFinished).Result;
        }

        public BulkDeleteResponse BatchDeleteDocuments(DateTime startDate, DateTime endDate, string projectName, DocumentType documentType)
        {
            var query = new SqlQuerySpec($"SELECT * FROM c WHERE c.documentType = '{documentType}' and c.projectName = '{projectName}' and (c.occurrenceDate between '{startDate.AddDays(-1):yyyy/MM/ddTHH:mm:ss}' and '{endDate:yyyy/MM/ddTHH:mm:ss}')");
            var documents = cosmosConnection.CreateDocumentQuery<CosmosDocumentBase>(cosmosDbContainer, query).ToList();
            if (documents.Count == 0)
                return new BulkDeleteResponse();

            var idsToDelete = new List<Tuple<string, string>>();
            foreach (var document in documents)
                idsToDelete.Add(new Tuple<string, string>(document.Id, document.Id));

            var summary = cosmosConnection.BulkDeleteDocuments(cosmosDbContainer, idsToDelete).Result;
            return summary;
        }
    }
}
