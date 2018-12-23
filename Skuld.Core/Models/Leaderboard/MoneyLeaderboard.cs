namespace Skuld.Core.Models
{
    public struct MoneyLeaderboardEntry
    {
        public ulong ID { get; set; }
        public ulong Money { get; set; }

        public static bool operator ==(MoneyLeaderboardEntry c1, MoneyLeaderboardEntry c2)
        {
            return c1.Equals(c2);
        }
        public static bool operator !=(MoneyLeaderboardEntry c1, MoneyLeaderboardEntry c2)
        {
            return !c1.Equals(c2);
        }
    }
}
