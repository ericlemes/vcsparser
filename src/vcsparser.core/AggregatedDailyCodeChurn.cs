using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class AggregatedDailyCodeChurn
    {
        public List<DailyCodeChurnAuthor> Authors { get; set; }
        public static readonly string DATE_FORMAT = "yyyy/MM/dd HH:mm:ss";

        public AggregatedDailyCodeChurn()
        {
            this.Authors = new List<DailyCodeChurnAuthor>();
            this.DailyCodeChurnPerFile = new List<DailyCodeChurn>();
        }
        public int UniqueAuthors
        {
            get { return Authors.Count; }
        }

        private string timestamp = "";
        public string Timestamp
        {
            get { return timestamp; }
            set { this.timestamp = value; }
        }

        public int Added { get; set; }

        public int AddedInFixesVCS { get; set; }

        public int AddedInFixesBugDB { get; set; }

        public int Deleted { get; set; }

        public int DeletedInFixesVCS { get; set; }

        public int DeletedInFixesBugDB { get; set; }

        public int ChangesBefore { get; set; }

        public int ChangesBeforeInFixesVCS { get; set; }

        public int ChangesBeforeInFixesBugDB { get; set; }

        public int ChangesAfter { get; set; }

        public int ChangesAfterInFixesVCS { get; set; }

        public int ChangesAfterInFixesBugDB { get; set; }

        public int TotalLinesChanged
        {
            get
            {
                return Added + Deleted + ChangesAfter + ChangesBefore;
            }
        }
        public int TotalLinesChangedInFixesVCS
        {
            get
            {
                return AddedInFixesVCS + DeletedInFixesVCS + ChangesAfterInFixesVCS + ChangesBeforeInFixesVCS;
            }
        }

        public int TotalLinesChangedInFixesBugDB
        {
            get
            {
                return AddedInFixesBugDB + DeletedInFixesBugDB + ChangesAfterInFixesBugDB + ChangesBeforeInFixesBugDB;
            }
        }

        public int NumberOfChanges { get; set; }

        public int NumberOfChangesWithFixesVCS { get; set; }

        public int NumberOfChangesWithFixesBugDB { get; set; }

        public DateTime GetDateTimeAsDateTime()
        {
            return DateTime.ParseExact(this.Timestamp, DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        public List<DailyCodeChurn> DailyCodeChurnPerFile
        {
            get; set;
        }

    }
}
