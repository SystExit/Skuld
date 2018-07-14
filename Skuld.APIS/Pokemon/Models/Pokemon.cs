using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Skuld.APIS.Pokemon.Models
{
    public class Pokemon
    {
        [JsonProperty(PropertyName = "forms")]
        public UrlItem[] Forms { get; private set; }

        [JsonProperty(PropertyName = "abilities")]
        public Abilities[] Abilities { get; private set; }

        [JsonProperty(PropertyName = "stats")]
        public Stats[] Stats { get; private set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }

        [JsonProperty(PropertyName = "weight")]
        public int Weight { get; private set; }

        [JsonProperty(PropertyName = "moves")]
        public Moves[] Moves { get; private set; }

        [JsonProperty(PropertyName = "sprites")]
        public Sprites Sprites { get; private set; }

        [JsonProperty(PropertyName = "held_items")]
        public HeldItems[] HeldItems { get; private set; }

        [JsonProperty(PropertyName = "location_area_encounters")]
        public string AreaEncounters { get; private set; }

        [JsonProperty(PropertyName = "height")]
        public int Height { get; private set; }

        [JsonProperty(PropertyName = "is_default")]
        public bool Default { get; private set; }

        [JsonProperty(PropertyName = "species")]
        public UrlItem Species { get; private set; }

        [JsonProperty(PropertyName = "id")]
        public int PokeID { get; private set; }

        [JsonProperty(PropertyName = "order")]
        public int Order { get; private set; }

        [JsonProperty(PropertyName = "game_indices")]
        public GameIndices[] GameIndices { get; private set; }

        [JsonProperty(PropertyName = "base_experience")]
        public int BaseExperience { get; private set; }

        [JsonProperty(PropertyName = "types")]
        public Types[] Types { get; private set; }
    }

    public class Abilities
    {
        [JsonProperty(PropertyName = "slot")]
        public int Slot { get; private set; }

        [JsonProperty(PropertyName = "is_hidden")]
        public bool Hidden { get; private set; }

        [JsonProperty(PropertyName = "ability")]
        public UrlItem Ability { get; private set; }
    }

    public class Stats
    {
        [JsonProperty(PropertyName = "stat")]
        public UrlItem Stat { get; private set; }

        [JsonProperty(PropertyName = "effort")]
        public int Effort { get; private set; }

        [JsonProperty(PropertyName = "base_stat")]
        public int BaseStat { get; private set; }
    }

    public class Moves
    {
        [JsonProperty(PropertyName = "version_group_details")]
        public VersionGroupDetails[] VersionGroup { get; private set; }

        [JsonProperty(PropertyName = "move")]
        public UrlItem Move { get; private set; }
    }

    public class VersionGroupDetails
    {
        [JsonProperty(PropertyName = "move_learn_method")]
        public UrlItem LearnableMoves { get; private set; }

        [JsonProperty(PropertyName = "level_learned_at")]
        public int LevelLearnedAt { get; private set; }

        [JsonProperty(PropertyName = "version_group")]
        public UrlItem GameVersion { get; private set; }
    }

    public class Sprites
    {
        [JsonProperty(PropertyName = "back_female")]
        public string BackFemale { get; private set; }

        [JsonProperty(PropertyName = "back_shiny_female")]
        public string BackShinyFemale { get; private set; }

        [JsonProperty(PropertyName = "back_default")]
        public string BackMale { get; private set; }

        [JsonProperty(PropertyName = "front_female")]
        public string FrontFemale { get; private set; }

        [JsonProperty(PropertyName = "front_shiny_female")]
        public string FrontShinyFemale { get; private set; }

        [JsonProperty(PropertyName = "back_shiny")]
        public string BackShinyMale { get; private set; }

        [JsonProperty(PropertyName = "front_default")]
        public string FrontMale { get; private set; }

        [JsonProperty(PropertyName = "front_shiny")]
        public string FrontShinyMale { get; private set; }
    }

    public class HeldItems
    {
        [JsonProperty(PropertyName = "item")]
        public UrlItem Item { get; private set; }

        [JsonProperty(PropertyName = "version_details")]
        public VersionDetails[] VersionDetails { get; private set; }
    }

    public class VersionDetails
    {
        [JsonProperty(PropertyName = "version")]
        public UrlItem Version { get; private set; }

        [JsonProperty(PropertyName = "rarity")]
        public int Rarity { get; private set; }
    }

    public class GameIndices
    {
        [JsonProperty(PropertyName = "version")]
        public UrlItem Version { get; set; }

        [JsonProperty(PropertyName = "game_index")]
        public int GameIndex { get; set; }
    }

    public class Types
    {
        [JsonProperty(PropertyName = "slot")]
        public int Slot { get; private set; }

        [JsonProperty(PropertyName = "type")]
        public UrlItem Type { get; private set; }
    }

    public class UrlItem
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; private set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }
    }
}