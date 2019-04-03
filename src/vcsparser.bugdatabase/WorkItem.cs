using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.bugdatabase
{
    public class WorkItemList
    {
        public int TotalWorkItems { get; set; }
        public IEnumerable<WorkItem> WorkItems { get; set; }
        public int ValidWorkItems { get; set; }
        public int ValidWorkItemsFlaggedAsBug { get; set; }
    }

    public class WorkItem
    {
        public DateTime ClosedDate { get; set; }
        public int WorkItemId { get; set; }
        public DateTime? ChangesetDate { get; set; }
        public bool ValidChangeset { get; set; }
        public bool FlaggedAsBug { get; set; }
        public string IntegrationBuild { get; set; }
        public string Message { get; set; }
    }
}
