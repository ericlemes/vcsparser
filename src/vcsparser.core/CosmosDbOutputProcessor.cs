using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core
{
    public class CosmosDbOutputProcessor : IOutputProcessor, ICosmosDbOutputProcessor
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
            this.projectName = projectName.Replace("\n", "").Replace("\r", "");
        }

        public void ProcessOutput<T>(Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            var listOfLists = codeChurnDataMapper.ConvertDictToOrderedListPerDay(dict);
            logger.LogToConsole(listOfLists.Count + " files with changes");

            var codeChurnFilesPerDay = listOfLists.SelectMany(x => x.Value.Select(y => y.Value as DailyCodeChurn)).ToList();

            DeleteExistingData(codeChurnFilesPerDay);

            foreach (var list in listOfLists)
            {

                foreach (var dailyCodeChurn in list.Value.Select(document => document.Value as DailyCodeChurn))
                {
                    dailyCodeChurn.LongName = projectName + "_" + "code-churn" + "_" + list.Key.ToString(DailyCodeChurn.DATE_FORMAT);
                    cosmosConnection.CreateDocument(codeChurnContainer, dailyCodeChurn).Wait();
                }
            }
        }

        public Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> GetDocumentsBasedOnDateRange(DateTime fromDateTime, DateTime endDateTime)
        {
            var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.Timestamp between '{ fromDateTime.ToString(DailyCodeChurn.DATE_FORMAT) }' and '{ endDateTime.ToString(DailyCodeChurn.DATE_FORMAT) }' order by c.Timestamp desc");
            var result = cosmosConnection.CreateDocumentQuery<DailyCodeChurn>(codeChurnContainer, sqlQuery).ToList();
            var codeChurnData = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>();

            foreach (var dailyCodeChurn in result)
            {
                var codeChurnDateTime = DateTime.Parse(dailyCodeChurn.Timestamp);

                if (codeChurnData.ContainsKey(codeChurnDateTime) == false)
                {
                    var codeChurn = new Dictionary<string, DailyCodeChurn>
                    {
                        { dailyCodeChurn.FileName, dailyCodeChurn }
                    };

                    codeChurnData.Add(codeChurnDateTime, codeChurn);
                }
                else
                    codeChurnData[codeChurnDateTime].Add(dailyCodeChurn.FileName, dailyCodeChurn);
            }

            return codeChurnData;
        }

        private void DeleteExistingData(List<DailyCodeChurn> dailyCodeChurnsList)
        {
            foreach (var dailyCodeChurn in dailyCodeChurnsList)
            {
                var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.LongName = '{ projectName + "_" + "code-churn" + "_" + dailyCodeChurn.GetDateTimeAsDateTime() }' and c.Timestamp = '{ dailyCodeChurn.Timestamp }' and c.FileName = '{ dailyCodeChurn.FileName }'");

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
