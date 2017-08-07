using PokeSharp.Models.Common;
using PokeSharp.Models.Pokemon;
using Newtonsoft.Json;

namespace PokeSharp.Models
{
    public class PocketMonster
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "base_experience")]
        public int? BaseExperience { get; set; }
        [JsonProperty(PropertyName = "height")]
        public int? Height { get; set; }
        [JsonProperty(PropertyName = "is_default")]
        public bool IsDefault { get; set; }
        [JsonProperty(PropertyName = "order")]
        public int? Order { get; set; }
        [JsonProperty(PropertyName = "weight")]
        public int? Weight { get; set; }
        [JsonProperty(PropertyName = "abilities")]
        public PokemonAbility[] Abilities { get; set; }
        [JsonProperty(PropertyName = "forms")]
        public NamedAPIResource[] Forms { get; set; }
        [JsonProperty(PropertyName = "game_indices")]
        public VersionGameIndex[] GameIndices { get; set; }
        [JsonProperty(PropertyName = "held_items")]
        public PokemonHeldItem[] HeldItems { get; set; }
        [JsonProperty(PropertyName = "location_area_encounters")]
        public string LocationAreaEncounters { get; set; }
        [JsonProperty(PropertyName = "moves")]
        public PokemonMove[] Moves { get; set; }
        [JsonProperty(PropertyName = "sprites")]
        public PokemonSprites Sprites { get; set; }
        [JsonProperty(PropertyName = "species")]
        public NamedAPIResource Species { get; set; }
        [JsonProperty(PropertyName = "stats")]
        public PokemonStat[] Stats { get; set; }
        [JsonProperty(PropertyName = "types")]
        public PokemonType[] Types { get; set; }
    }
}
