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
    public interface IBugDatabaseProcessor
    {
        Dictionary<DateTime, Dictionary<string, WorkItem>> ProcessBugDatabase(string dllPath, IEnumerable<string> dllArgs);
        void ProcessCache(string cacheDirectory, IChangesetProcessor changesetProcessor);
    }

    public class BugDatabaseProcessor : IBugDatabaseProcessor
    {
        private readonly IBugDatabaseDllLoader bugDatabaseDllLoader;
        private readonly IWorkItemConverter workItemConverter;
        private readonly IWebRequest webRequest;

        private readonly IFileSystem fileSystem;
        private readonly IJsonListParser<WorkItem> workItemParser;
        private readonly ILogger logger;

        public BugDatabaseProcessor(IBugDatabaseDllLoader bugDatabaseDllLoader, IWorkItemConverter workItemConverter, IWebRequest webRequest, IFileSystem fileSystem, IJsonListParser<WorkItem> workItemParser, ILogger logger)
        {
            this.bugDatabaseDllLoader = bugDatabaseDllLoader;
            this.workItemConverter = workItemConverter;
            this.webRequest = webRequest;

            this.fileSystem = fileSystem;
            this.workItemParser = workItemParser;
            this.logger = logger;
        }

        public Dictionary<DateTime, Dictionary<string, WorkItem>> ProcessBugDatabase(string dllPath, IEnumerable<string> dllArgs)
        {
            if (string.IsNullOrWhiteSpace(dllPath) ||
                dllArgs == null)
                return null;

            string path = Path.GetFullPath(dllPath);
            IBugDatabaseProvider databaseProvider = bugDatabaseDllLoader.Load(path, dllArgs, webRequest);
            if (databaseProvider == null)
                return null;

            return databaseProvider.Process();
        }

        public void ProcessCache(string cacheOutput, IChangesetProcessor changesetProcessor)
        {
            if (changesetProcessor == null)
                return;

            var cacheDirectory = Directory.GetParent(cacheOutput).FullName;

            var files = fileSystem.GetFiles(cacheDirectory, "*.json");

            foreach (var file in files)
            {
                logger.LogToConsole($"Processing {file.FileName}");
                var workItemList = workItemParser.ParseFile(file.FileName);
                var changesets = workItemConverter.Convert(workItemList);
                foreach (var changeset in changesets)
                    changesetProcessor.ProcessBugDatabaseChangeset(changeset);
            }
        }
    }
}
