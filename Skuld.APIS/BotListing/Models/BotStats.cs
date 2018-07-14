using Newtonsoft.Json;

namespace Skuld.APIS.BotListing.Models
{
    public class BotStats
    {
        [JsonProperty(PropertyName = "server_count")]
        public int ServerCount { get; set; }

        [JsonProperty(PropertyName = "shard_id")]
        public int ShardID { get; set; }

        [JsonProperty(PropertyName = "shard_count")]
        public int ShardCount { get; set; }
    }
}