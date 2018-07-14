using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class Owner
    {
        [JsonProperty(PropertyName = "id")]
        public string ID { get; set; }
    }
}