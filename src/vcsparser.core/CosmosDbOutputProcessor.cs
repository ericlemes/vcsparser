using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using vcsparser.core.Database.Cosmos;
using vcsparser.core.Database.Repository;

namespace vcsparser.core
{
    public class CosmosDbOutputProcessor : IOutputProcessor
    {
        private readonly IDataDocumentRepository dataDocumentRepository;
        private readonly ILogger logger;
        private readonly string projectName;

        public CosmosDbOutputProcessor(ILogger logger, IDataDocumentRepository dataDocumentRepository, string projectName)
        {
            this.logger = logger;
            this.dataDocumentRepository = dataDocumentRepository;
            this.projectName = projectName;
        }

        public void ProcessOutput<T>(OutputType outputType, string outputFile, Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            var listOfFilesPerDay = (from data in dict from valueValue in data.Value.Values select ConvertOutputJsonToCosmosDataDocument(valueValue, GetDocumentType<T>(), data.Key)).ToList();
           
            logger.LogToConsole($"Found: {listOfFilesPerDay.Count} documents to upload");

            var documentsDeleted = dataDocumentRepository.DeleteMultipleDocuments(listOfFilesPerDay);

            logger.LogToConsole($"Deleted: {documentsDeleted} existing documents");

            foreach (var document in listOfFilesPerDay)
                dataDocumentRepository.CreateDataDocument(document);
        }

        public Dictionary<DateTime, Dictionary<string, T>> GetDocumentsInDateRange<T>(DateTime fromDateTime, DateTime endDateTime) where T : IOutputJson
        {
            var documents = dataDocumentRepository.GetDocumentsInDateRange<T>(GetDocumentType<T>(), fromDateTime, endDateTime);
            var data = new Dictionary<DateTime, Dictionary<string, T>>();

            if (documents == null || documents.Count == 0)
            {
                logger.LogToConsole($"Could not find any documents in range: {fromDateTime} to {endDateTime}");
                return data;
            }

            logger.LogToConsole($"Found: {data.Count} documents in range: {fromDateTime} to {endDateTime}");

            foreach (var cosmosDocument in documents)
            {
                var documentDate = DateTime.ParseExact(cosmosDocument.DateTime, CosmosDataDocument<T>.DATE_FORMAT, CultureInfo.InvariantCulture);
                if (!data.ContainsKey(documentDate))
                {
                    var outputData = cosmosDocument.Data.ToDictionary(fileData => fileData.FileName);
                    data.Add(documentDate, outputData);
                }
                else
                    foreach (var existingData in cosmosDocument.Data)
                        data[documentDate].Add(existingData.FileName, existingData);
            }

            return data;
        }

        private CosmosDataDocument<T> ConvertOutputJsonToCosmosDataDocument<T>(T data, DocumentType documentType, DateTime occurrenceDate) where T : IOutputJson
        {
            return new CosmosDataDocument<T> { Data = new List<T> { data }, DocumentName = $"{projectName}_{documentType}_{occurrenceDate:yyyy-MM-dd}", DocumentType = documentType, DateTime = occurrenceDate.ToString(CosmosDataDocument<T>.DATE_FORMAT, CultureInfo.InvariantCulture)};
        }

        private DocumentType GetDocumentType<T>()
        {
            return typeof(T) == typeof(DailyCodeChurn) ? DocumentType.CodeChurn : DocumentType.BugDatabase;
        }
    }
}
