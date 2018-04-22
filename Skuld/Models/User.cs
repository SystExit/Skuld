using System.Threading.Tasks;

namespace Skuld.Models
{
    public class SkuldUser
    {
        public ulong ID { get; set; }
        public ulong Money { get; set; }
		public string Language { get; set; }
        public string Description { get; set; }
        public string Daily { get; set; }
        public double LuckFactor { get; set; }
        public bool DMEnabled { get; set; }
        public uint Petted { get; set; }
        public uint Pets { get; set; }
        public uint HP { get; set; }
        public uint GlaredAt { get; set; }
        public uint Glares { get; set; }
        public string FavCmd { get; set; }
        public ulong FavCmdUsg { get; set; }
    }
}
