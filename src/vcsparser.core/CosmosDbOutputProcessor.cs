using System;
using System.Collections.Generic;
using System.Linq;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core
{
    public class CosmosDbOutputProcessor : OutputProcessor
    {
        private readonly ICosmosConnection cosmosConnection;
        private readonly ILogger logger;
        private readonly string projectName;
        private readonly string codeChurnContainer;

        public CosmosDbOutputProcessor(ILogger logger, ICosmosConnection cosmosConnection, string codeChurnContainer, string projectName)
        {
            this.logger = logger;
            this.cosmosConnection = cosmosConnection;
            this.codeChurnContainer = codeChurnContainer;
            this.projectName = projectName;
        }

        public override void ProcessOutput<T>(OutputType outputType, string outputFile, Dictionary<DateTime, Dictionary<string, T>> dict)
        {
            var listOfLists = ConvertDictToOrderedListPerDay(dict);
            logger.LogToConsole(listOfLists.Count + " files with changes");

            foreach (var list in listOfLists)
            {
                var codeChurnCosmosDocument = new CodeChurnDocument
                {
                    Id = FormatDocumentId(list),
                    Data = list.Value.Values.Cast<DailyCodeChurn>().ToList()
                };
                cosmosConnection.CreateDocument(codeChurnContainer, codeChurnCosmosDocument).Wait();
            }
        }
        private string FormatDocumentId<T>(KeyValuePair<DateTime, SortedList<T, T>> list)
        {
            return projectName + "_" + "code-churn" + "_" + list.Key.ToString("yyyy-MM-dd");
        }
    }
}
