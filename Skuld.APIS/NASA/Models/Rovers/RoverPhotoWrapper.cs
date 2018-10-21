using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Skuld.APIS.NASA.Models
{
    public class RoverPhotoWrapper
    {
        [JsonProperty(PropertyName = "photos")]
        public RoverPhoto[] Photos { get; internal set; }
    }
}
