using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface IBugDatabaseProcessor
    {
        /// <summary>
        /// Process the provided bug database and return the Work Items found
        /// </summary>
        /// <param name="dllPath">The file path to the dll</param>
        /// <param name="dllArgs">List of arguments to pass to the dll</param>
        /// <returns>Dictionary indexed by DateTime, indexed by changeset id</returns>
        Dictionary<DateTime, Dictionary<string, WorkItem>> ProcessBugDatabase(string dllPath, IEnumerable<string> dllArgs);
        void ProcessCache(IChangesetProcessor changesetProcessor);
    }
}
