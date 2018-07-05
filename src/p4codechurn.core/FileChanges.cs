using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public class FileChanges
    {
        public string FileName { get; set; }

        public int Added { get; set; }

        public int Deleted { get; set; }

        public int ChangedBefore { get; set; }

        public int ChangedAfter { get; set; }
    }
}
