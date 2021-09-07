using System;
using System.Collections.Generic;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core
{
    public class CosmosDbOutputProcessor : OutputProcessor
    {
        private readonly ICosmosConnection cosmosConnection;
        private readonly ILogger logger;
        private readonly string filePrefix;

        public CosmosDbOutputProcessor(ILogger logger, ICosmosConnection cosmosConnection)
        {
            this.logger = logger;
            this.cosmosConnection = cosmosConnection;
        }

        public override void ProcessOutput<T>(OutputType outputType, string outputFile, Dictionary<DateTime, Dictionary<string, T>> dict)
        {
            var listOfLists = ConvertDictToOrderedListPerDay(dict);
            logger.LogToConsole(listOfLists.Count + " files to cosmos db");

            foreach (var list in listOfLists)
                UploadDocumentToCosmosDb(filePrefix + "_" + list.Key.ToString("yyyy-MM-dd") + ".json", list.Value.Values);
        }

        private void UploadDocumentToCosmosDb<T>(string fileId, IList<T> values)
        {
            foreach (var document in values)
            {
                cosmosConnection.CreateDocument("CodeChurn", document).Wait();
            }
        }
    }
}
