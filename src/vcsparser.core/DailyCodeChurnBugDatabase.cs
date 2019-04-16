using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class DailyCodeChurnBugDatabase
    {
        public int NumberOfChanges { get; set; }

        public int NumberOfChangesWithFixes { get; set; }

        public int Added { get; set; }

        public int Deleted { get; set; }

        public int ChangesBefore { get; set; }

        public int ChangesAfter { get; set; }

        public int TotalLinesChanged {
            get {
                return Added + Deleted + ChangesAfter + ChangesBefore;
            }
        }
    }
}
