using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class ImageNode
    {
        [JsonProperty(PropertyName = "node")]
        public Image Node { get; set; }
    }
}