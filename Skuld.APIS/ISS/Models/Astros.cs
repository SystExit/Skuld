using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Skuld.APIS.ISS.Models
{
    public class Astros
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; internal set; }

        [JsonProperty(PropertyName = "number")]
        public int NumberOfAstronauts { get; internal set; }

        [JsonProperty(PropertyName = "people")]
        public AstroCraft[] Astronauts { get; internal set; }
    }
}