using PokeSharp.Models.Common;
using PokeSharp.Models.Items;
using Newtonsoft.Json;

namespace PokeSharp.Models
{
    public class Item
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "cost")]
        public int? Cost { get; set; }
        [JsonProperty(PropertyName = "fling_power")]
        public int? FlingPower { get; set; }
        [JsonProperty(PropertyName = "fling_effect")]
        public NamedAPIResource FlingEffect { get; set; }
        [JsonProperty(PropertyName = "attributes")]
        public NamedAPIResource[] Attributes { get; set; }
        [JsonProperty(PropertyName = "category")]
        public ItemCategory Category { get; set; }
        [JsonProperty(PropertyName = "effect_entries")]
        public VerboseEffect[] EffectEntries { get; set; }
        [JsonProperty(PropertyName = "flavor_text_entries")]
        public VersionGroupFlavorText[] FlavorTextEntries { get; set; }
        [JsonProperty(PropertyName = "game_indices")]
        public GenerationGameIndex[] GameIndices { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "sprites")]
        public ItemSprites Sprites { get; set; }
        [JsonProperty(PropertyName = "held_by_pokemon")]
        public ItemHolderPokemon[] HeldByPokemon { get; set; }
        [JsonProperty(PropertyName = "baby_trigger_for")]
        public APIResource BabyTriggerFor {get;set;}
        [JsonProperty(PropertyName = "machines")]
        public MachineVersionDetail[] Machines { get; set; }
    }
}
