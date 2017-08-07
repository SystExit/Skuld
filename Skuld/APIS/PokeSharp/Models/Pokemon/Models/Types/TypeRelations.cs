using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class TypeRelations
    {
        [JsonProperty(PropertyName = "no_damage_to")]
        public NamedAPIResource[] NoDamageTo { get; set; }
        [JsonProperty(PropertyName = "half_damage_to")]
        public NamedAPIResource[] HalfDamageTo { get; set; }
        [JsonProperty(PropertyName = "double_damage_to")]
        public NamedAPIResource[] DoubleDamageTo { get; set; }
        [JsonProperty(PropertyName = "no_damage_from")]
        public NamedAPIResource[] NoDamageFrom { get; set; }
        [JsonProperty(PropertyName = "half_damage_from")]
        public NamedAPIResource[] HalfDamageFrom { get; set; }
        [JsonProperty(PropertyName = "double_damage_from")]
        public NamedAPIResource[] DoubleDamageFrom { get; set; }
    }
}
