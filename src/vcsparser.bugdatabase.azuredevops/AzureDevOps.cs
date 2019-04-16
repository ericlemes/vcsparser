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
        Dictionary<DateTime, Dictionary<string, WorkItem>> GetWorkItems();
    }

    public class AzureDevOps : IAzureDevOps
    {
        private readonly ILogger logger;
        private readonly IAzureDevOpsRequest request;
        private readonly IApiConverter apiConverter;
        private readonly ITimeKeeper timeKeeper;

        private object _lock;

        public AzureDevOps(ILogger logger, IAzureDevOpsRequest request, IApiConverter apiConverter, ITimeKeeper timeKeeper)
        {
            this.logger = logger;
            this.request = request;
            this.apiConverter = apiConverter;
            this.timeKeeper = timeKeeper;

            this._lock = new object();
        }

        private void ProcessWorkItem(Dictionary<DateTime, Dictionary<string, WorkItem>> workItems, JSONQueryItem item)
        {
            try
            {
                var fullWorkItem = request.GetFullWorkItem(item.Url).Result;
                WorkItem workItem = apiConverter.ConvertToWorkItem(fullWorkItem);

                lock (_lock)
                {
                    if (!workItems.ContainsKey(workItem.ClosedDate.Date))
                        workItems.Add(workItem.ClosedDate.Date, new Dictionary<string, WorkItem>());

                    workItems[workItem.ClosedDate.Date].Add(workItem.ChangesetId, workItem);
                }
            }
            catch (Exception e)
            {
                logger.LogToConsole($"Error Processing Work Item '{item.Id}': {(e.InnerException == null ? e.Message : e.InnerException.Message)}");
            }
        }

        private Dictionary<DateTime, Dictionary<string, WorkItem>> ProcessWorkItemList(JSONQueryItem[] items)
        {
            var workItems = new Dictionary<DateTime, Dictionary<string, WorkItem>>();
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

        public Dictionary<DateTime, Dictionary<string, WorkItem>> GetWorkItems()
        {
            logger.LogToConsole($"AzureDevOps BugDatabase");
            var json = request.GetWorkItemList().Result;
            return ProcessWorkItemList(json.WorkItems);
        }
    }
}
