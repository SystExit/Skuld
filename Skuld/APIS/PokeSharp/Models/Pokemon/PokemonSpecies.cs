using PokeSharp.Models.Common;
using Newtonsoft.Json;

namespace PokeSharp.Models.Pokemon
{
    public class PokemonSpecies
    {
        [JsonProperty(PropertyName = "id")]
        public int? ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "order")]
        public int? Order { get; set; }
        [JsonProperty(PropertyName = "gender_rate")]
        public int? GenderRate { get; set; }
        [JsonProperty(PropertyName = "capture_rate")]
        public int? CaptureRate { get; set; }
        [JsonProperty(PropertyName = "base_happiness")]
        public int? BaseHappiness { get; set; }
        [JsonProperty(PropertyName = "is_baby")]
        public bool IsBaby { get; set; }
        [JsonProperty(PropertyName = "hatch_counter")]
        public int? HatchCounter { get; set; }
        [JsonProperty(PropertyName = "has_gender_differences")]
        public bool HasGenderDifferences { get; set; }
        [JsonProperty(PropertyName = "forms_switchable")]
        public bool SwitchableForms { get; set; }
        [JsonProperty(PropertyName = "growth_rate")]
        public NamedAPIResource GrowthRate { get; set; }
        [JsonProperty(PropertyName = "pokedex_numbers")]
        public PokemonSpeciesDexEntry[] PokeDexNumbers { get; set; }
        [JsonProperty(PropertyName = "egg_groups")]
        public NamedAPIResource[] EggGroups { get; set; }
        [JsonProperty(PropertyName = "color")]
        public NamedAPIResource Color { get; set; }
        [JsonProperty(PropertyName = "shape")]
        public NamedAPIResource Shape { get; set; }
        [JsonProperty(PropertyName = "evolves_from_species")]
        public NamedAPIResource EvolvesFromSpecies { get; set; }
        [JsonProperty(PropertyName = "evolution_chain")]
        public NamedAPIResource EvolutionChain { get; set; }
        [JsonProperty(PropertyName = "habitat")]
        public NamedAPIResource Habitat { get; set; }
        [JsonProperty(PropertyName = "generation")]
        public NamedAPIResource Generation { get; set; }
        [JsonProperty(PropertyName = "names")]
        public ListName[] Names { get; set; }
        [JsonProperty(PropertyName = "pal_park_encounters")]
        public PalParkEncounterArea[] PalParkEncounters { get; set; }
        [JsonProperty(PropertyName = "flavor_text_entries")]
        public FlavorTextList[] FlavorTextEntries { get; set; }
        [JsonProperty(PropertyName = "form_descriptions")]
        public ListDescription[] FormDescriptions { get; set; }
        [JsonProperty(PropertyName = "genera")]
        public ListGenus[] Genera { get; set; }
        [JsonProperty(PropertyName = "varieties")]
        public PokemonSpeciesVariety[] Varieties { get; set; }
    }
}
