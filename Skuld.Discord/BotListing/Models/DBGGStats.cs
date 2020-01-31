using Newtonsoft.Json;

namespace Skuld.Discord.BotListing.Models
{
    public class DBGGStats
    {
        [JsonProperty(PropertyName = "guildCount")]
        public int ServerCount { get; set; }

        [JsonProperty(PropertyName = "shardId", NullValueHandling = NullValueHandling.Ignore)]
        public int? ShardID { get; set; }

        [JsonProperty(PropertyName = "shardCount", NullValueHandling = NullValueHandling.Ignore)]
        public int? ShardCount { get; set; }

        public static explicit operator BotStats(DBGGStats stats)
        {
            return new BotStats
            {
                ServerCount = stats.ServerCount,
                ShardID = stats.ShardID,
                ShardCount = stats.ShardCount
            };
        }
    }
}