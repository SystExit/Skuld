namespace Skuld.Core.Models.Skuld
{
    public class GuildExperience
    {
        public ulong GuildID { get; set; }
        public ulong Level { get; set; }
        public ulong XP { get; set; }
        public ulong TotalXP { get; set; }
        public ulong LastGranted { get; set; }
    }
}
