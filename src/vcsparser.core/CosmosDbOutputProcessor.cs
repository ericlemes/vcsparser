using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core
{
    public class CosmosDbOutputProcessor : IOutputProcessor
    {
        private readonly ICosmosConnection cosmosConnection;
        private readonly ICodeChurnDataMapper codeChurnDataMapper;
        private readonly ILogger logger;
        private readonly string projectName;
        private readonly string codeChurnContainer;

        public CosmosDbOutputProcessor(ILogger logger, ICosmosConnection cosmosConnection, ICodeChurnDataMapper codeChurnDataMapper, string codeChurnContainer, string projectName)
        {
            this.logger = logger;
            this.cosmosConnection = cosmosConnection;
            this.codeChurnDataMapper = codeChurnDataMapper;
            this.codeChurnContainer = codeChurnContainer;
            this.projectName = projectName;
        }

        public void ProcessOutput<T>(Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            var listOfLists = codeChurnDataMapper.ConvertDictToOrderedListPerDay(dict);
            logger.LogToConsole(listOfLists.Count + " files with changes");

            GetDocumentsBasedOnDateRange(DateTime.Parse("2020/12/10"), DateTime.Parse("2020/12/15"));

            var allDailyCodeChurnFiles = listOfLists.SelectMany(x => x.Value.Select(y => y.Value as DailyCodeChurn))
                .ToList();

            DeleteExistingData(allDailyCodeChurnFiles);

            foreach (var list in listOfLists)
            {
                foreach (var dailyCodeChurn in list.Value.Select(document => document.Value as DailyCodeChurn))
                {
                    dailyCodeChurn.LongName = projectName + "_" + "code-churn" + "_" + list.Key.ToString("yyyy-MM-dd");
                    cosmosConnection.CreateDocument(codeChurnContainer, dailyCodeChurn).Wait();
                }
            }
        }

        public List<DailyCodeChurn> GetDocumentsBasedOnDateRange(DateTime from, DateTime to)
        {

            var fromDateTime = from.ToString("yyyy-MM-dd").Replace("-", "/");
            var toDateTime = to.ToString("yyyy-MM-dd").Replace("-", "/");

            var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.Timestamp between '{fromDateTime}' and '{toDateTime}'");

            var result = cosmosConnection.CreateDocumentQuery<DailyCodeChurn>(codeChurnContainer, sqlQuery).ToList();

            return result;
        }

        private void DeleteExistingData(List<DailyCodeChurn> dailyCodeChurnsList)
        {
           
            foreach (var dailyCodeChurn in dailyCodeChurnsList)
            {
                var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.LongName = '{ projectName + "_" + "code-churn" + "_" + dailyCodeChurn.GetDateTimeAsDateTime().ToString("yyyy-MM-dd") }' and c.Timestamp = '{ dailyCodeChurn.Timestamp }' and c.FileName = '{ dailyCodeChurn.FileName }'");

                var result = cosmosConnection.CreateDocumentQuery<DailyCodeChurn>(codeChurnContainer, sqlQuery).ToList();
                if (result.Count <= 0) continue;

                foreach (var codeChurnDocument in result)
                {
                    cosmosConnection.DeleteDocument(codeChurnContainer, codeChurnDocument.Id);
                }
            }
        }
    }
}
