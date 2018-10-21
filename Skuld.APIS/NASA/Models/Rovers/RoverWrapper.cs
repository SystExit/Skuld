using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Skuld.APIS.NASA.Models
{
    public class RoverWrapper
    {
        [JsonProperty(PropertyName = "rover")]
        public Rover Rover { get; internal set; }
    }
}
