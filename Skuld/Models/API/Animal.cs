using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace Skuld.Models.API
{
    public class Animal
    {
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "file")]
        public string FileUrl { get; set; }
    }
}
