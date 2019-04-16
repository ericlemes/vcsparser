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
        void ProcessCache(ChangesetProcessor changesetProcessor);
    }

    public class BugDatabaseProcessor : IBugDatabaseProcessor
    {
        private readonly IBugDatabaseDllLoader bugDatabaseDllLoader;
        private readonly IWorkItemConverter workItemConverter;
        private readonly IWebRequest webRequest;

        public BugDatabaseProcessor(IBugDatabaseDllLoader bugDatabaseDllLoader, IWorkItemConverter workItemConverter, IWebRequest webRequest)
        {
            this.bugDatabaseDllLoader = bugDatabaseDllLoader;
            this.workItemConverter = workItemConverter;
            this.webRequest = webRequest;
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

        public void ProcessCache(ChangesetProcessor changesetProcessor)
        {
            if (changesetProcessor == null)
                return;

            var workItems = new Dictionary<DateTime, IDictionary<string, WorkItem>>();
            // TODO Read work items into program

            IDictionary<DateTime, IDictionary<string, IChangeset>> changesets = workItemConverter.Convert(workItems);

            foreach (var changesetsByDate in changesets)
                foreach (var changesetById in changesetsByDate.Value)
                    changesetProcessor.ProcessBugDatabaseChangeset(changesetById.Value);
        }
    }
}
