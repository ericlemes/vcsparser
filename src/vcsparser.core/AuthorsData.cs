using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class AuthorsData
    {
        [JsonProperty("authors")]
        public List<DailyCodeChurnAuthor> Authors { get; set; }

        [JsonProperty("date")]
        public string Timestamp { get; set; }
    }
}
