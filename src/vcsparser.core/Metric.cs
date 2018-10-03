using Newtonsoft.Json;

namespace vcsparser.core
{
    public class Metric
    {
        [JsonProperty("key")]
        public string MetricKey { get; set; }

        [JsonProperty("name")]
        public string Name{ get; set; }

        [JsonProperty("type")]
        public string Type { get { return "INT"; } }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("direction")]
        public int Direction { get { return 0; } }

        [JsonProperty("qualitative")]
        public bool Qualitative { get { return false; } }

        [JsonProperty("domain")]
        public string Domain { get { return "Code churn"; } }
    }
}