using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class UserFeed
    {
        [JsonProperty(PropertyName = "user")]
        public InstagramUser User { get; set; }
    }
}