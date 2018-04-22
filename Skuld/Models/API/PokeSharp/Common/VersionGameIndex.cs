using Newtonsoft.Json;
namespace PokeSharp.Models.Common
{
    public class VersionGameIndex
    {
        [JsonProperty(PropertyName = "game_index")]
        public int? GameIndex { get; set; }
        [JsonProperty(PropertyName = "version")]
        public NamedAPIResource Version { get; set; }
    }
}
