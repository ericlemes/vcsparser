using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class DailyCodeChurnAuthor
    {
        public string Author
        {
            get; set;
        }

        public int NumberOfChanges
        {
            get;
            set;
        }
    }
}
