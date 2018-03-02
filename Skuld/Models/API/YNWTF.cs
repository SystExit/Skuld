using Newtonsoft.Json;

namespace Skuld.Models.API
{
    public class YNWTF
    {
        [JsonProperty(PropertyName = "answer")]
        public string Answer;
        [JsonProperty(PropertyName = "image")]
        public string Image;
    }
}
