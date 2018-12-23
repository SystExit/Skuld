namespace Skuld.Core.Models
{
    public struct GuildLeaderboardEntry
    {
        public ulong ID { get; set; }
        public ulong XP { get; set; }
        public ulong TotalXP { get; set; }
        public ulong Level { get; set; }

        public static bool operator ==(GuildLeaderboardEntry c1, GuildLeaderboardEntry c2)
        {
            return c1.Equals(c2);
        }
        public static bool operator !=(GuildLeaderboardEntry c1, GuildLeaderboardEntry c2)
        {
            return !c1.Equals(c2);
        }
    }
}
