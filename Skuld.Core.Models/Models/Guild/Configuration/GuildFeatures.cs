namespace Skuld.Core.Models
{
    public class GuildFeatures
    {
        public ulong Id { get; set; }
        public bool Pinning { get; set; } = false;
        public bool Experience { get; set; } = false;
    }
}