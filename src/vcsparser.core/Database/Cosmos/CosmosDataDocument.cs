using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace vcsparser.core.Database.Cosmos
{
    public class CosmosDataDocument<T> : CosmosDocumentBase where T : IOutputJson
    {
        public static readonly string DATE_FORMAT = "yyyy/MM/dd HH:mm:ss";

        [JsonProperty("data")]
        public List<T> Data { get; set; }

        [JsonProperty("documentName")]
        public string DocumentName { get; set; }

        [JsonProperty("occurrenceDate")]
        public string DateTime { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("documentType")]
        public DocumentType DocumentType { get; set; }
    }
}
