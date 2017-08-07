using PokeSharp.Models.Common;
using Newtonsoft.Json;
namespace PokeSharp.Models.Evolution
{
    public class ChainLink
    {
        [JsonProperty(PropertyName = "is_baby")]
        public bool IsBaby { get; set; }
        [JsonProperty(PropertyName = "species")]
        public NamedAPIResource Species { get; set; }
        [JsonProperty(PropertyName = "evolution_details")]
        public EvolutionDetail[] EvolutionDetails { get; set; }
        [JsonProperty(PropertyName = "evolves_to")]
        public ChainLink[] EvolvesTo { get; set; }
    }
}
