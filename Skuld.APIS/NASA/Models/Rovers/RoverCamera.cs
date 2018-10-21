using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Skuld.APIS.NASA.Models
{
    public class RoverCamera
    {
        [JsonProperty(PropertyName = "id")]
        public int ID { get; internal set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; internal set; }

        [JsonProperty(PropertyName = "rover_id")]
        public int RoverID { get; internal set; }

        [JsonProperty(PropertyName = "full_name")]
        public string FullName { get; internal set; }
    }
}
