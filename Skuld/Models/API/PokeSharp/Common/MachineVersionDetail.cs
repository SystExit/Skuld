using Newtonsoft.Json;
namespace PokeSharp.Models.Common
{
    public class MachineVersionDetail        
    {
        [JsonProperty(PropertyName = "machine")]
        public APIResource Machine { get; set; }
        [JsonProperty(PropertyName = "version_group")]
        public NamedAPIResource VersionGroup { get; set; }
    }
}
