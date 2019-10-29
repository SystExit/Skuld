using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Skuld.APIS.NASA.Models
{
    public class Rover
    {
        [JsonProperty(PropertyName = "id")]
        public int ID { get; internal set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; internal set; }

        [JsonProperty(PropertyName = "landing_date")]
        public string LandingDate { get; internal set; }

        [JsonProperty(PropertyName = "launch_date")]
        public string LaunchDate { get; internal set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; internal set; }

        [JsonProperty(PropertyName = "max_sol")]
        public int MaxSOL { get; internal set; }

        [JsonProperty(PropertyName = "max_date")]
        public string MaxDate { get; internal set; }

        [JsonProperty(PropertyName = "total_photos")]
        public int TotalPhotos { get; internal set; }

        [JsonProperty(PropertyName = "cameras")]
        public NameFullNamePair[] Cameras { get; internal set; }
    }
}