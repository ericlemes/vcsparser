using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class DailyCodeChurnBugDatabase
    {
        public int NumberOfChangesInFixes { get; set; }

        public int AddedInFixes { get; set; }

        public int DeletedInFixes { get; set; }

        public int ChangesBeforeInFixes { get; set; }

        public int ChangesAfterInFixes { get; set; }

        public int TotalLinesChangedInFixes {
            get {
                return AddedInFixes + DeletedInFixes + ChangesAfterInFixes + ChangesBeforeInFixes;
            }
        }
    }
}
