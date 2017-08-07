using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Encounters
{
    public class EncounterMethod
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "order")]
        public int? Order { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
    }
}
