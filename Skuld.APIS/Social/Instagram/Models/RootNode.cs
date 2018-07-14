using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class RootNode
    {
        [JsonProperty(PropertyName = "entry_data")]
        public EntryData EntryData { get; set; }
    }
}