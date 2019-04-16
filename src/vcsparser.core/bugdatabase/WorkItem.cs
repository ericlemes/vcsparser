using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public class WorkItem : IComparable, IOutputJson
    {
        public string ClosedDate { get; set; }
        public string WorkItemId { get; set; }
        public string ChangesetId { get; set; }

        public int CompareTo(object obj)
        {
            WorkItem dest = (WorkItem)obj;

            var dates = this.ClosedDate.CompareTo(dest.ClosedDate);
            if (dates != 0)
                return dates;
            else
                return this.WorkItemId.CompareTo(dest.WorkItemId);
        }

        public void SetClosedDateFromDateTime(DateTime value)
        {
            this.ClosedDate = value.ToString(DailyCodeChurn.DATE_FORMAT, CultureInfo.InvariantCulture);
        }

        public DateTime GetClosedDateAsDateTime()
        {
            return DateTime.ParseExact(this.ClosedDate, DailyCodeChurn.DATE_FORMAT, CultureInfo.InvariantCulture);
        }
    }
}
