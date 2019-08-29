namespace Skuld.Core.Models.Skuld
{
    public class IAmRole
    {
        public ulong GuildId;
        public ulong RoleId;
        public uint Price;
        public uint LevelRequired;
        public ulong RequiredRoleId;
    }
    public enum IAmFail
    {
        Success,
        Price,
        Level,
        RequiredRole
    }
}
