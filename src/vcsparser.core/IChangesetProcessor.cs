using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.core
{
    public interface IChangesetProcessor
    {
        Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> Output { get; }
        Dictionary<string, List<WorkItem>> WorkItemCache { get; }
        int ChangesetsWithBugs { get; }

        void ProcessChangeset(IChangeset changeset);
    }
}
