using System.Collections.Generic;
using Newtonsoft.Json;

namespace vcsparser.core.Database.Cosmos
{
    public class CodeChurnDocument : CosmosDocumentBase
    {
        [JsonProperty("data")]
        public List<DailyCodeChurn> Data { get; set; }
    }
}
