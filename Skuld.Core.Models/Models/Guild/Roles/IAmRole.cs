namespace Skuld.Core.Models
{
    public class IAmRole
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public int Price { get; set; }
        public int LevelRequired { get; set; }
        public ulong RequiredRoleId { get; set; }
    }
}