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
        void Process(ChangesetProcessor changesetProcessor, string dllPath, IEnumerable<string> dllArgs);
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

        public void Process(ChangesetProcessor changesetProcessor, string dllPath, IEnumerable<string> dllArgs)
        {
            if (changesetProcessor == null ||
                string.IsNullOrWhiteSpace(dllPath) ||
                dllArgs == null)
                return;

            string path = Path.GetFullPath(dllPath);
            IBugDatabaseProvider databaseProvider = bugDatabaseDllLoader.Load(path, dllArgs, webRequest);
            if (databaseProvider == null)
                return;

            var workItems = databaseProvider.Process();
            IEnumerable<IChangeset> changesets = workItemConverter.Convert(workItems.WorkItems);

            foreach (var changeset in changesets)
                changesetProcessor.ProcessBugDatabaseChangeset(changeset);
        }
    }
}
