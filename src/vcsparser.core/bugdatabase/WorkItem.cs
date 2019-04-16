using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public class WorkItemList
    {
        public int TotalWorkItems { get; set; }
        public IEnumerable<WorkItem> WorkItems { get; set; }
    }

    public class WorkItem
    {
        public DateTime ClosedDate { get; set; }
        public string WorkItemId { get; set; }
        public string ChangesetId { get; set; }
    }
}
