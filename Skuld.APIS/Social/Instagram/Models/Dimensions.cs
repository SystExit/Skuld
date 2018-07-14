using Newtonsoft.Json;

namespace Skuld.APIS.Social.Instagram.Models
{
    public class Dimensions
    {
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }
    }
}