using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace vcsparser.core
{
    public interface IMeasure
    {
        string MetricKey { get; set; }

        string File { get; set; }
    }

    public class Measure<T> : IMeasure
    {
        [JsonProperty("metric-key")]
        public string MetricKey { get; set; }

        [JsonProperty("file", NullValueHandling = NullValueHandling.Ignore)]
        public string File { get; set; }

        [JsonProperty("value")]
        public T Value { get; set; }
    }
}