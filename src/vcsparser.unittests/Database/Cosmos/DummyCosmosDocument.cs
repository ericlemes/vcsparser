using Newtonsoft.Json;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.unittests.Database.Cosmos
{
    public sealed class DummyCosmosDocument : CosmosDocumentBase
    {
        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }
    }
}
