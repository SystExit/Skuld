namespace Skuld.Core.Models
{
    public class ExperienceLeaderboardEntry : LeaderboardEntry
    {
        public ulong XP { get; set; }
        public ulong TotalXP { get; set; }
        public ulong Level { get; set; }
    }
}
