using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class WindowRoot
    {
        [JsonProperty(PropertyName = "entry_data")]
        public InstagramUser User { get; set; }
    }
}