using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using vcsparser.core.Database.Cosmos;
using vcsparser.core.Database.Repository;
using vcsparser.core.Helpers;

namespace vcsparser.core
{
    public class CosmosDbOutputProcessor : IOutputProcessor
    {
        private readonly IDataDocumentRepository dataDocumentRepository;
        private readonly ILogger logger;
        private readonly IDataConverter dataConverter;
        private readonly string projectName;
        private readonly int batchSize;

        public CosmosDbOutputProcessor(ILogger logger, IDataDocumentRepository dataDocumentRepository, IDataConverter dataConverter, string projectName, int batchSize)
        {
            this.logger = logger;
            this.dataDocumentRepository = dataDocumentRepository;
            this.dataConverter = dataConverter;
            this.projectName = projectName;
            this.batchSize = batchSize;
        }

        public void ProcessOutput<T>(OutputType outputType, string outputFile, Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            dict = dict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            logger.LogToConsole($"Started inserting documents for ('{projectName}') project of '({DocumentTypeHelper.GetDocumentType<T>()}') type.");

            var listOfFilesPerDay = dataConverter.ConvertDictToOrderedListPerDay(dict);
            var cosmosDocumentsToInsert = (from x in listOfFilesPerDay from valueValue in x.Value.Values select ConvertOutputJsonToCosmosDataDocument(valueValue, DocumentTypeHelper.GetDocumentType<T>(), x.Key)).ToList();

            if (cosmosDocumentsToInsert.Count >= batchSize)
            {
                logger.LogToConsole("Inserting documents with batches.");

                BatchDeleteDocuments<T>(dict.First().Key, dict.Last().Key);
                BatchInsertDocuments(cosmosDocumentsToInsert);
            }
            else
            {
                DeleteDocuments(cosmosDocumentsToInsert);
                InsertDocuments(cosmosDocumentsToInsert);
            }

            logger.LogToConsole($"Finished inserting documents to cosmos for ('{projectName}') project of '({DocumentTypeHelper.GetDocumentType<T>()}') type.");
        }

        public Dictionary<DateTime, Dictionary<string, T>> GetDocumentsInDateRange<T>(DateTime fromDateTime, DateTime endDateTime) where T : IOutputJson
        {
            logger.LogToConsole($"Requesting for ('{projectName}') documents of '({DocumentTypeHelper.GetDocumentType<T>()}') type in date range: {fromDateTime} to {endDateTime}.");

            var documents = dataDocumentRepository.GetDocumentsInDateRange<T>(projectName, DocumentTypeHelper.GetDocumentType<T>(), fromDateTime, endDateTime);
            var data = ConvertDataFromCosmosDb(documents);

            logger.LogToConsole($"Received {data.Count} ('{projectName}') documents of '({DocumentTypeHelper.GetDocumentType<T>()}') type in date range: {fromDateTime} to {endDateTime}.");

            return data;
        }

        public Dictionary<DateTime, Dictionary<string, T>> GetAllDocumentsByProjectNameAndDocumentType<T>() where T : IOutputJson
        {
            logger.LogToConsole($"Requesting for all ('{projectName}') documents of '({DocumentTypeHelper.GetDocumentType<T>()}') type.");

            var documents = dataDocumentRepository.GetAllDocumentsByProjectAndDocumentType<T>(projectName, DocumentTypeHelper.GetDocumentType<T>());
            var data = ConvertDataFromCosmosDb(documents);

            logger.LogToConsole($"Received {data.Count} ('{projectName}') documents of '({DocumentTypeHelper.GetDocumentType<T>()}') type.");

            return data;
        }

        private Dictionary<DateTime, Dictionary<string, T>> ConvertDataFromCosmosDb<T>(List<CosmosDataDocument<T>> documents) where T : IOutputJson
        {
            var data = new Dictionary<DateTime, Dictionary<string, T>>();

            if (documents == null || documents.Count == 0)
            {
                logger.LogToConsole($"[Converting Cosmos Data] Could not find any documents for project: ('{projectName}')  of ('{DocumentTypeHelper.GetDocumentType<T>()}') type.");
                return data;
            }

            logger.LogToConsole($"[Converting Cosmos Data] Found: {data.Count} documents for project:  ('{projectName}')  of ('{DocumentTypeHelper.GetDocumentType<T>()}') type.");
            
            ConvertDataFromCosmosDb(documents, data);
            
            logger.LogToConsole($"[Converting Cosmos Data] Finished converting cosmos data. Returning {data.Count} ('{projectName}') documents with data of ('{DocumentTypeHelper.GetDocumentType<T>()}') type.");

            return data;
        }

        private void ConvertDataFromCosmosDb<T>(List<CosmosDataDocument<T>> documents, Dictionary<DateTime, Dictionary<string, T>> data) where T : IOutputJson
        {
            foreach (var cosmosDocument in documents)
            {
                var documentDate = DateTime.ParseExact(cosmosDocument.DateTime, CosmosDataDocument<T>.DATE_FORMAT, CultureInfo.InvariantCulture);
                if (!data.ContainsKey(documentDate))
                    AddNewDataItem(cosmosDocument, documentDate, data);
                else
                    AddExistingDataToTheSameDay(cosmosDocument, documentDate, data);
            }
        }

        private void AddNewDataItem<T>(CosmosDataDocument<T> cosmosDocument, DateTime documentDate, Dictionary<DateTime, Dictionary<string, T>> data) where T : IOutputJson
        {
            logger.LogToConsole($"[Converting Cosmos Data] Found new document day with data. Day: ('{documentDate}')");
            var outputData = cosmosDocument.Data.ToDictionary(fileData => fileData.GetFileLongName());
            data.Add(documentDate, outputData);
        }

        private void AddExistingDataToTheSameDay<T>(CosmosDataDocument<T> cosmosDocument, DateTime documentDate, Dictionary<DateTime, Dictionary<string, T>> data) where T : IOutputJson
        {
            foreach (var existingData in cosmosDocument.Data)
            {
                logger.LogToConsole($"[Converting Cosmos Data] Adding a new document to existing day: ('{documentDate}'))");
                data[documentDate].Add(existingData.GetFileLongName(), existingData);
            }
        }

        private CosmosDataDocument<T> ConvertOutputJsonToCosmosDataDocument<T>(T data, DocumentType documentType, DateTime occurrenceDate) where T : IOutputJson
        {
            return new CosmosDataDocument<T> { Data = new List<T> { data }, ProjectName = projectName, DocumentType = documentType, DateTime = occurrenceDate.ToString(CosmosDataDocument<T>.DATE_FORMAT, CultureInfo.InvariantCulture)};
        }

        private void InsertDocuments<T>(List<CosmosDataDocument<T>> listOfFilesPerDay) where T : IOutputJson
        {
            logger.LogToConsole($"Found: {listOfFilesPerDay.Count}  ('{projectName}') documents of ('{DocumentTypeHelper.GetDocumentType<T>()}') type to upload.");

            foreach (var document in listOfFilesPerDay)
                dataDocumentRepository.CreateDataDocument(document);

            logger.LogToConsole($"Inserted: {listOfFilesPerDay.Count}  ('{projectName}') documents of ('{DocumentTypeHelper.GetDocumentType<T>()}') type.");
        }

        private void DeleteDocuments<T>(List<CosmosDataDocument<T>> listOfFilesPerDay) where T : IOutputJson
        {
            logger.LogToConsole($"Deleting: existing ('{projectName}') documents of ('{DocumentTypeHelper.GetDocumentType<T>()}') type.");

            var documentsDeleted = dataDocumentRepository.DeleteMultipleDocuments(listOfFilesPerDay);

            logger.LogToConsole($"Deleted: {documentsDeleted} existing ('{projectName}') documents of ('{DocumentTypeHelper.GetDocumentType<T>()}') type.");
        }

        private void BatchInsertDocuments<T>(List<CosmosDataDocument<T>> listOfFilesPerDay) where T : IOutputJson
        {
            logger.LogToConsole($"Starting to batch upload {listOfFilesPerDay.Count}  ('{projectName}') doc(s) to cosmos of ('{DocumentTypeHelper.GetDocumentType<T>()}') type.");

            var summary = dataDocumentRepository.BatchInsertCosmosDocuments(listOfFilesPerDay, LogBatchDetails);

            logger.LogToConsole($"Finished upload of {summary.NumberOfDocumentsInserted}  ('{projectName}')  doc(s) of ('{DocumentTypeHelper.GetDocumentType<T>()}') type in {summary.TotalTimeTaken.TotalSeconds} sec(s).");
        }

        private void BatchDeleteDocuments<T>(DateTime fromDateTime, DateTime toDateTime) where T : IOutputJson
        {
            logger.LogToConsole($"Deleting existing ('{projectName}') documents between: {fromDateTime} and {toDateTime} of ('{DocumentTypeHelper.GetDocumentType<T>()}') type.");

            var deleteSummary = dataDocumentRepository.BatchDeleteDocuments(fromDateTime, toDateTime, projectName, DocumentTypeHelper.GetDocumentType<T>());

            logger.LogToConsole($"Finished deleting of ('{projectName}') {deleteSummary.NumberOfDocumentsDeleted} existing documents in {deleteSummary.TotalTimeTaken} sec(s) of ('{DocumentTypeHelper.GetDocumentType<T>()}') type.");
        }

        [ExcludeFromCodeCoverage]
        private void LogBatchDetails(CosmosBulkImportSummary batchSummary)
        {
            logger.LogToConsole($"Inserted batch {batchSummary.NumberOfBatchesCompleted}/{batchSummary.NumberOfBatches} " +
                                $"{batchSummary.NumberOfDocumentsInserted} doc(s) in {batchSummary.TotalTimeTaken.TotalSeconds} sec(s)");
        }
    }
}
