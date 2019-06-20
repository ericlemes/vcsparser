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
        [JsonProperty("author")]
        public string Author
        {
            get; set;
        }

        [JsonProperty("number_of_changes")]
        public int NumberOfChanges
        {
            get;
            set;
        }
    }
}
