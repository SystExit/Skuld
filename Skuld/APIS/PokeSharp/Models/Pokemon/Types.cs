using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class Types
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "damage_relations")]
        public TypeRelations DamageRelations { get; set; }
        [JsonProperty(PropertyName = "game_indices")]
        public GenerationGameIndex[] GameIndicies { get; set; }
        [JsonProperty(PropertyName = "generation")]
        public NamedAPIResource Generation { get; set; }
        [JsonProperty(PropertyName = "move_damage_class")]
        public NamedAPIResource MoveDamageClass { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "pokemon")]
        public TypePokemon[] Pokemon { get; set; }
        [JsonProperty(PropertyName = "moves")]
        public NamedAPIResource[] Moves { get; set; }
    }
}
