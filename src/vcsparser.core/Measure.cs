﻿using Newtonsoft.Json;

namespace p4codechurn.core
{
    public class Measure
    {
        [JsonProperty("metric-key")]
        public string MetricKey { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }
}