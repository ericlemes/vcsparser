using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core
{
    public class CosmosDbOutputProcessor : IOutputProcessor, ICosmosDbOutputProcessor
    {
        private readonly ICosmosConnection cosmosConnection;
        private readonly ILogger logger;
        private readonly string projectName;
        private readonly string cosmosDbContainer;

        public CosmosDbOutputProcessor(ILogger logger, ICosmosConnection cosmosConnection, string cosmosDbContainer, string projectName)
        {
            this.logger = logger;
            this.cosmosConnection = cosmosConnection;
            this.cosmosDbContainer = cosmosDbContainer;
            this.projectName = projectName;
        }

        public void ProcessOutput<T>(Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            var documentType = typeof(T) == typeof(DailyCodeChurn) ? DocumentType.CodeChurn : DocumentType.BugDatabase;  
            var listOfFilesPerDay = (from data in dict from valueValue in data.Value.Values select ConvertOutputJsonToCosmosDataDocument(valueValue, documentType, data.Key)).ToList();
           
            logger.LogToConsole($"Found: {listOfFilesPerDay.Count} documents to upload");

            DeleteExistingData(listOfFilesPerDay);

            foreach (var document in listOfFilesPerDay)
            {
                cosmosConnection.CreateDocument(cosmosDbContainer, document).Wait();
            }
        }

        //public Dictionary<DateTime, Dictionary<string, T>> GetDocumentsBasedOnDateRange<T>(DateTime fromDateTime, DateTime endDateTime) where T : IOutputJson
        //{
        //    var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.Timestamp between '{ fromDateTime.ToString(DailyCodeChurn.DATE_FORMAT) }' and '{ endDateTime.ToString(DailyCodeChurn.DATE_FORMAT) }' order by c.Timestamp desc");
        //    var result = cosmosConnection.CreateDocumentQuery<T>(cosmosDbContainer, sqlQuery).ToList();
        //    var codeChurnData = new Dictionary<DateTime, Dictionary<string, T>>();

        //    foreach (var dailyCodeChurn in result)
        //    {
        //        var codeChurnDateTime = DateTime.Parse(dailyCodeChurn.Timestamp);

        //        if (codeChurnData.ContainsKey(codeChurnDateTime) == false)
        //        {
        //            var codeChurn = new Dictionary<string, DailyCodeChurn>
        //            {
        //                { dailyCodeChurn.FileName, dailyCodeChurn }
        //            };

        //            codeChurnData.Add(codeChurnDateTime, codeChurn);
        //        }
        //        else
        //            codeChurnData[codeChurnDateTime].Add(dailyCodeChurn.FileName, dailyCodeChurn);
        //    }

        //    return codeChurnData;
        //}

        private void DeleteExistingData<T>(List<CosmosDataDocument<T>> documentsToDelete) where T : IOutputJson
        {
            foreach (var documentToDelete in documentsToDelete)
            {
                var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.documentType= '{documentToDelete.DocumentType}' and c.documentName = '{documentToDelete.DocumentName}' and c.occurrenceDate = '{documentToDelete.OccurrenceDate:yyyy-MM-ddTHH:mm:ss}'");

                var result = cosmosConnection.CreateDocumentQuery<CosmosDataDocument<T>>(cosmosDbContainer, sqlQuery).ToList();
                if (result.Count <= 0) continue;

                foreach (var codeChurnDocument in result)
                {
                    cosmosConnection.DeleteDocument(cosmosDbContainer, codeChurnDocument.Id).Wait();
                }
            }
        }

        private CosmosDataDocument<T> ConvertOutputJsonToCosmosDataDocument<T>(T data, DocumentType documentType, DateTime occurrenceDate) where T : IOutputJson
        {
            return new CosmosDataDocument<T> { Data = new List<T> { data }, DocumentName = $"{projectName}_{documentType}_{occurrenceDate:yyyy-MM-dd}", DocumentType = documentType, OccurrenceDate = occurrenceDate.ToString(CosmosDataDocument<T>.DATE_FORMAT, CultureInfo.InvariantCulture)};
        }
    }
}
