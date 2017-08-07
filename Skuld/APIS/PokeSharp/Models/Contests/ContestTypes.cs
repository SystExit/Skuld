using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Contests
{
    public class ContestTypes
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "berry_flavor")]
        public NamedAPIResource BerryFlavour { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ContestName Names { get; set; }
    }
}
