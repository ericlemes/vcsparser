using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace vcsparser.core.Database.Cosmos
{
    public class CosmosDocumentBase
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("_etag", NullValueHandling = NullValueHandling.Ignore)]
        public string ETag { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty("_ts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("_self", NullValueHandling = NullValueHandling.Ignore)]
        public string SelfLink { get; set; }

        [JsonProperty("_rid", NullValueHandling = NullValueHandling.Ignore)]
        public string ResourceId { get; set; }
    }
}
