namespace Skuld.Core.Models
{
    public class LevelRewards
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public int LevelRequired { get; set; }
    }
}