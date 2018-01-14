namespace Skuld.Models
{
    public class SkuldGuild
    {
        public ulong ID { get; set; }
        public string Name { get; set; }
        public string JoinMessage { get; set; }
        public string LeaveMessage { get; set; }
        public ulong AutoJoinRole { get; set; }
        public string Prefix { get; set; }
		public string Language { get; set; }
        public string[] JoinableRoles { get; set; }
        public ulong TwitchNotifChannel { get; set; }
        public ulong TwitterLogChannel { get; set; }
        public ulong MutedRole { get; set; }
        public ulong AuditChannel { get; set; }
        public ulong UserJoinChannel { get; set; }
        public ulong UserLeaveChannel { get; set; }
        public ulong StarboardChannel { get; set; }
        public GuildSettings GuildSettings { get; set; }
    }

    public class GuildSettings
    {
        public GuildCommandModules Modules { get; set; }
        public GuildFeatureModules Features { get; set; }
    }
    public class GuildCommandModules
    {
        public bool AccountsEnabled { get; set; }
        public bool ActionsEnabled { get; set; }
        public bool AdminEnabled { get; set; }
        public bool FunEnabled { get; set; }
        public bool HelpEnabled { get; set; }
        public bool InformationEnabled { get; set; }
        public bool SearchEnabled { get; set; }
        public bool StatsEnabled { get; set; }
    }
    public class GuildFeatureModules
    {
        public bool Starboard { get; set; }
        public bool Pinning { get; set; }
        public bool Experience { get; set; }
        public bool UserJoinLeave { get; set; }
        public bool UserModification { get; set; }
        public bool UserBanEvents { get; set; }
        public bool GuildModification { get; set; }
        public bool GuildChannelModification { get; set; }
        public bool GuildRoleModification { get; set; }
    }
}
