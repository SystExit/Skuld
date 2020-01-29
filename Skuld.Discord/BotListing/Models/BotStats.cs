using Newtonsoft.Json;

namespace Skuld.Discord.BotListing.Models
{
    public class BotStats
    {
        [JsonProperty(PropertyName = "server_count")]
        public int ServerCount { get; set; }

        [JsonProperty(PropertyName = "shard_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? ShardID { get; set; }

        [JsonProperty(PropertyName = "shard_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? ShardCount { get; set; }

        public static explicit operator DBGGStats(BotStats stats)
        {
            return new DBGGStats
            {
                ServerCount = stats.ServerCount,
                ShardID = stats.ShardID,
                ShardCount = stats.ShardCount
            };
        }
    }
}