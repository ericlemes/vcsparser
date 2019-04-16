using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using System.Text.RegularExpressions;
using vcsparser.core.p4;
using vcsparser.core.git;
using System.Threading;

namespace vcsparser.bugdatabase.azuredevops
{
    public interface IAzureDevOps
    {
        WorkItemList GetWorkItems();
    }

    public class AzureDevOps : IAzureDevOps
    {
        private readonly ILogger logger;
        private readonly IAzureDevOpsRequest request;
        private readonly IApiConverter apiConverter;
        private readonly ITimeKeeper timeKeeper;

        public AzureDevOps(ILogger logger, IAzureDevOpsRequest request, IApiConverter apiConverter, ITimeKeeper timeKeeper)
        {
            this.logger = logger;
            this.request = request;
            this.apiConverter = apiConverter;
            this.timeKeeper = timeKeeper;
        }

        private void ProcessWorkItem(List<WorkItem> workItems, JSONQueryItem item)
        {
            try
            {
                var fullWorkItem = request.GetFullWorkItem(item.Url).Result;
                var workItem = apiConverter.ConvertToWorkItem(fullWorkItem);
                workItems.Add(workItem);
            }
            catch (Exception e)
            {
                logger.LogToConsole($"Error Processing Work Item '{item.Id}': {(e.InnerException == null ? e.Message : e.InnerException.Message)}");
            }
        }

        private IEnumerable<WorkItem> ProcessWorkItemList(JSONQueryItem[] items)
        {
            List<WorkItem> workItems = new List<WorkItem>();
            timeKeeper.IntervalAction = () => logger.LogToConsole($"Finished processing {workItems.Count}/{items.Length} Work Items");
            timeKeeper.Start();
            var workItemsTasks = items.Select(
                item => Task.Run(
                    () => ProcessWorkItem(workItems, item)
                )
            );

            Task.WaitAll(workItemsTasks.ToArray());
            timeKeeper.Cancel();
            return workItems;
        }

        public WorkItemList GetWorkItems()
        {
            logger.LogToConsole($"AzureDevOps BugDatabase");
            var json = request.GetWorkItemList().Result;
            var itemList = ProcessWorkItemList(json.WorkItems).ToArray();
            WorkItemList workItemList = new WorkItemList
            {
                TotalWorkItems = itemList.Length,
                WorkItems = itemList
            };
            return workItemList;
        }
    }
}
