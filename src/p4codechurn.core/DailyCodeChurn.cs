using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public class DailyCodeChurn : IComparable
    {
        public DateTime Timestamp { get; set; }

        public string FileName { get; set; }

        public string Extension
        {
            get
            {
                return Path.GetExtension(FileName);
            }
        }

        public int Added { get; set; }

        public int Deleted { get; set; }

        public int ChangesBefore { get; set; }

        public int ChangesAfter { get; set; }
        public int TotalLinesChanged
        {
            get
            {
                return Added + Deleted + Math.Max(ChangesAfter, ChangesBefore);
            }
        }

        public int CompareTo(object obj)
        {
            DailyCodeChurn dest = (DailyCodeChurn)obj;

            var dates = this.Timestamp.CompareTo(dest.Timestamp);
            if (dates != 0)
                return dates;
            else
                return this.FileName.CompareTo(dest.FileName);                    
        }
    }
}
