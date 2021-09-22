using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<CosmosDataDocument<T>> GetDocumentsInDateRange<T>(DocumentType documentType, DateTime fromDateTime, DateTime endDateTime) where T : IOutputJson
        {
            var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.documentType = '{documentType}' and (c.occurrenceDate between '{ fromDateTime.ToString(CosmosDataDocument<T>.DATE_FORMAT) }' and '{ endDateTime.ToString(CosmosDataDocument<T>.DATE_FORMAT) }') order by c.occurrenceDate desc");
            return cosmosConnection.CreateDocumentQuery<CosmosDataDocument<T>>(cosmosDbContainer, sqlQuery).ToList();
        }

        public int DeleteMultipleDocuments<T>(List<CosmosDataDocument<T>> documentsToDelete) where T : IOutputJson
        {
            var deletedDocuments = 0;
            foreach (var documentToDelete in documentsToDelete)
            {
                var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.documentType= '{documentToDelete.DocumentType}' and c.documentName = '{documentToDelete.DocumentName}' and c.occurrenceDate = '{documentToDelete.DateTime:yyyy-MM-ddTHH:mm:ss}'");

                var result = cosmosConnection.CreateDocumentQuery<CosmosDataDocument<T>>(cosmosDbContainer, sqlQuery, null).ToList();if (result.Count <= 0) continue;

                foreach (var cosmosDataDocuments in result)
                {
                    deletedDocuments++;
                    cosmosConnection.DeleteDocument(cosmosDbContainer, cosmosDataDocuments.Id).Wait();

                }
            }

            return deletedDocuments;
        }
    }
}
