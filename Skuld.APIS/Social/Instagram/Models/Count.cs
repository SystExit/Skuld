using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class Count
    {
        [JsonProperty(PropertyName = "count")]
        public int Value { get; set; }
    }
}