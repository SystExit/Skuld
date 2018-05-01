using Newtonsoft.Json;

namespace PokeSharp.Models.Common
{
    public class APIResource
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get;  set; }
    }
}
