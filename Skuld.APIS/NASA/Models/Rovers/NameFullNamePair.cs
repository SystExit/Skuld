using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Skuld.APIS.NASA.Models
{
    public class NameFullNamePair
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; internal set; }

        [JsonProperty(PropertyName = "full_name")]
        public string FullName { get; internal set; }
    }
}
