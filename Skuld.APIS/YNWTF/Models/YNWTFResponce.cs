using Newtonsoft.Json;

namespace Skuld.APIS.YNWTF.Models
{
    public class YNWTFResponce
    {
        [JsonProperty(PropertyName = "answer")]
        public string Answer;

        [JsonProperty(PropertyName = "image")]
        public string Image;
    }
}