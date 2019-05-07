using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class JsonOutputData
    {
        public int SchemaVersion { get; set; }
        public IList<DailyCodeChurn> Data { get; set; }
    }
}
