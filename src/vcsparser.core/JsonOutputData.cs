using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.core
{
    public class JsonOutputData<T> where T : IOutputJson
    {
        public static readonly int DailyCodeChurn = 1;
        public static readonly int BugDatabase = 1;

        public static int CurrentVersion {
            get {
                if (typeof(T) == typeof(DailyCodeChurn)) return DailyCodeChurn;
                if (typeof(T) == typeof(WorkItem)) return BugDatabase;
                return 0;
            }
        }

        public int SchemaVersion { get; set; }
        public IList<T> Data { get; set; }
    }
}
