namespace Skuld.Core.Models
{
    public class SkuldUser
    {
        public ulong ID { get; set; }
        public bool Banned { get; set; }
        public string Description { get; set; }
        public bool CanDM { get; set; }
        public ulong Money { get; set; }
        public string Language { get; set; }
        public uint HP { get; set; }
        public uint Patted { get; set; }
        public uint Pats { get; set; }
        public uint GlaredAt { get; set; }
        public uint Glares { get; set; }
        public ulong Daily { get; set; }
        public string AvatarUrl { get; set; }
        public string FavCmd { get; set; }
        public ulong FavCmdUsg { get; set; }
    }
}