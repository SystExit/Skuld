using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class InstaText
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}