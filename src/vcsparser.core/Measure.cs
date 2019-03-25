using Newtonsoft.Json;

namespace vcsparser.core
{

    public abstract class Measure
    {
        [JsonProperty("metric-key")]
        public string MetricKey { get; set; }
        
        [JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
        public string File { get; set; }
    }

    public class Measure<T> : Measure
    {
        [JsonProperty("value")]
        public T Value { get; set; }
    }
}