using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;
using vcsparser.core.git;
using vcsparser.core.p4;

namespace vcsparser.core.bugdatabase
{
    public class BugDatabaseProcessor : IBugDatabaseProcessor
    {
        private readonly IBugDatabaseDllLoader bugDatabaseDllLoader;
        private readonly IWebRequest webRequest;

        private readonly IFileSystem fileSystem;
        private readonly IJsonListParser<WorkItem> workItemParser;
        private readonly ILogger logger;
        private readonly string cacheOutput;

        public BugDatabaseProcessor(IBugDatabaseDllLoader bugDatabaseDllLoader, IWebRequest webRequest, IFileSystem fileSystem, IJsonListParser<WorkItem> workItemParser, ILogger logger, string cacheOutput)
        {
            this.bugDatabaseDllLoader = bugDatabaseDllLoader;
            this.webRequest = webRequest;

            this.fileSystem = fileSystem;
            this.workItemParser = workItemParser;
            this.logger = logger;
            this.cacheOutput = cacheOutput;
        }

        public Dictionary<DateTime, Dictionary<string, WorkItem>> ProcessBugDatabase(string dllPath, IEnumerable<string> dllArgs)
        {
            string path = fileSystem.GetFullPath(dllPath);
            IBugDatabaseProvider databaseProvider = bugDatabaseDllLoader.Load(path, dllArgs, webRequest);
            return databaseProvider.Process();
        }

        public void ProcessCache(IChangesetProcessor changesetProcessor)
        {
            if (string.IsNullOrEmpty(cacheOutput))
                return;

            var files = fileSystem.GetFiles(fileSystem.GetParentFullName(cacheOutput), "*.json");

            foreach (var file in files)
            {
                logger.LogToConsole($"Processing {file.FileName}");
                var workItemList = workItemParser.ParseFile(file.FileName);
                foreach(var workItem in workItemList)
                {
                    if (workItem.ChangesetId == null) continue;
                    if (!changesetProcessor.WorkItemCache.ContainsKey(workItem.ChangesetId))
                        changesetProcessor.WorkItemCache.Add(workItem.ChangesetId, new List<WorkItem>());

                    changesetProcessor.WorkItemCache[workItem.ChangesetId].Add(workItem);
                }
            }
        }
    }
}
