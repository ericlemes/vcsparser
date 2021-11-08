using System;
using System.Collections.Generic;
using System.Linq;
using vcsparser.core.bugdatabase;
using vcsparser.core.Database.Cosmos;
using vcsparser.core.Helpers;

namespace vcsparser.core.Database
{
    public class DataFromFileToCosmosDb<T> : IDataFromFileToCosmosDb where T : IOutputJson
    {
        private readonly IFileSystem fileSystem;
        private readonly IOutputProcessor outputProcessor;
        private readonly ILogger logger;
        private readonly IJsonListParser<T> jsonListParser;
        private readonly string pathToJsonFiles;
        private readonly string cosmosProjectName;

        public DataFromFileToCosmosDb(ILogger logger, IFileSystem fileSystem, IOutputProcessor outputProcessor, IJsonListParser<T> jsonListParser, string pathToJsonFiles, string cosmosProjectName)
        {
            this.fileSystem = fileSystem;
            this.outputProcessor = outputProcessor;
            this.pathToJsonFiles = pathToJsonFiles;
            this.jsonListParser = jsonListParser;
            this.cosmosProjectName = cosmosProjectName;
            this.logger = logger;
        }

        public int Extract()
        {
            var files = fileSystem.GetFiles(pathToJsonFiles, "*.json").ToList();

            logger.LogToConsole($"Reading {files.Count} files from ('{pathToJsonFiles}') of ('{DocumentTypeHelper.GetDocumentType<T>()}') type.");
            var data = new Dictionary<DateTime, Dictionary<string, T>>();

            foreach (var file in files)
            {
                var codeChurnList = jsonListParser.ParseFile(file.FileName);
                logger.LogToConsole($"Processing file: ('{file.FileName}').");

                var outputData = codeChurnList.ToDictionary(fileData => fileData.GetFileLongName());
                logger.LogToConsole($"Found: {outputData.Keys.Count} data to insert.");

                data.Add(GetDateBasedOnDocumentType(codeChurnList), outputData);
            }

            outputProcessor.ProcessOutput(OutputType.CosmosDb, cosmosProjectName, data);

            logger.LogToConsole($"Finished processing files from: ('{pathToJsonFiles}').");

            return 0;
        }

        private DateTime GetDateBasedOnDocumentType(IList<T> data)
        {
            var documentType = DocumentTypeHelper.GetDocumentType<T>();

            if (documentType == DocumentType.BugDatabase)
            {
                var workItem = data[0] as WorkItem;
                return workItem.ClosedDate;
            }

            var dailyCodeChurn = data[0] as DailyCodeChurn;
            return dailyCodeChurn.GetDateTimeAsDateTime();
        }
    }
}
