using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Skuld.APIS.Models;

namespace Skuld.APIS.ISS.Models
{
    public class ISSPosition
    {
        [JsonProperty(PropertyName = "message")]
        public string Message { get; internal set; }

        [JsonProperty(PropertyName = "iis_position")]
        public LatLong IISPosition { get; internal set; }

        [JsonProperty(PropertyName = "timestamp")]
        public ulong Timestamp { get; internal set; }
    }
}
