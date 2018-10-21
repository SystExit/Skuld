using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Skuld.APIS.NASA.Models
{
    public class RoverPhoto
    {
        [JsonProperty(PropertyName = "id")]
        public int ID { get; internal set; }

        [JsonProperty(PropertyName = "sol")]
        public int SOL { get; internal set; }

        [JsonProperty(PropertyName = "camera")]
        public RoverCamera Camera { get; internal set; }

        [JsonProperty(PropertyName = "img_src")]
        public string ImageUrl { get; internal set; }

        [JsonProperty(PropertyName = "earth_date")]
        public string EarthDate { get; internal set; }

        [JsonProperty(PropertyName = "rover")]
        public Rover Rover { get; internal set; }
    }
}
