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
        public List<DailyCodeChurnAuthor> Authors { get; set; }
        [JsonIgnore]
        public DateTime Timestamp { get; set; }
        public string Date { get => Timestamp.ToString("yyyy/MM/dd"); }
    }
}
