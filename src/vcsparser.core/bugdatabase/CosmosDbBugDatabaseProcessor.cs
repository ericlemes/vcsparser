using System;
using System.Collections.Generic;
using vcsparser.core.Database.Cosmos;
using vcsparser.core.Database.Repository;

namespace vcsparser.core.bugdatabase
{
    public class CosmosDbBugDatabaseProcessor : IBugDatabaseProcessor
    {
        private readonly IBugDatabaseDllLoader bugDatabaseDllLoader;
        private readonly IWebRequest webRequest;

        private readonly IFileSystem fileSystem;
        private readonly ILogger logger;
        private readonly IDataDocumentRepository dataDocumentRepository;
        private readonly string projectName;

        public CosmosDbBugDatabaseProcessor(IBugDatabaseDllLoader bugDatabaseDllLoader, IFileSystem fileSystem, IWebRequest webRequest, ILogger logger, IDataDocumentRepository dataDocumentRepository, string projectName)
        {
            this.bugDatabaseDllLoader = bugDatabaseDllLoader;
            this.webRequest = webRequest;
            this.fileSystem = fileSystem;
            this.logger = logger;
            this.dataDocumentRepository = dataDocumentRepository;
            this.projectName = projectName;
        }

        public Dictionary<DateTime, Dictionary<string, WorkItem>> ProcessBugDatabase(string dllPath, IEnumerable<string> dllArgs)
        {
            var path = fileSystem.GetFullPath(dllPath);
            var databaseProvider = bugDatabaseDllLoader.Load(path, dllArgs, webRequest);
            return databaseProvider.Process();
        }

        public void ProcessCache(IChangesetProcessor changesetProcessor)
        {
            var workItemDocuments = dataDocumentRepository.GetAllDocumentsByProjectAndDocumentType<WorkItem>(projectName, DocumentType.BugDatabase);

            logger.LogToConsole($"Processing {workItemDocuments.Count} work items of {projectName} project");

            foreach (var workDocument in workItemDocuments)
            {
                foreach (var workItemList in workDocument.Data)
                {
                    if (workItemList.ChangesetId == null) continue;
                    if (!changesetProcessor.WorkItemCache.ContainsKey(workItemList.ChangesetId))
                        changesetProcessor.WorkItemCache.Add(workItemList.ChangesetId, new List<WorkItem>());

                    changesetProcessor.WorkItemCache[workItemList.ChangesetId].Add(workItemList);
                }
            }
        }
    }
}
