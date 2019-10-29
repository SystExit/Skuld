namespace Skuld.Core.Models
{
    public class UserExperience
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public ulong Level { get; set; }
        public ulong XP { get; set; }
        public ulong TotalXP { get; set; }
        public ulong LastGranted { get; set; }
    }
}