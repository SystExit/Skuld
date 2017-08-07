using PokeSharp.Models.Common;

namespace PokeSharp.Models
{
    public class Machine
    {
        public int? ID { get; set; }
        public NamedAPIResource Item { get; set; }
        public NamedAPIResource Move { get; set; }
        public NamedAPIResource VersionGroup { get; set; }
    }
}
