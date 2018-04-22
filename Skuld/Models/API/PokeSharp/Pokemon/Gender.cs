using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class Gender
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "pokemon_species_details")]
        public PokemonSpeciesGender[] PokemonSpeciesDetails { get; set; }
        [JsonProperty(PropertyName = "required_for_evolution")]
        public NamedAPIResource[] RequiredForEvolution { get; set; }
    }
}
