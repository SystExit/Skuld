using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class ProfilePage
    {
        [JsonProperty(PropertyName = "graphql")]
        public UserFeed Feeds { get; set; }
    }
}