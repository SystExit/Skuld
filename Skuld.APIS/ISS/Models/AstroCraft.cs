using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Skuld.APIS.ISS.Models
{
    public class AstroCraft
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; internal set; }

        [JsonProperty(PropertyName = "craft")]
        public string Craft { get; internal set; }
    }
}