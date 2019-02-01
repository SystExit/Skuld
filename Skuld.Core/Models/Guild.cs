using System.Collections.Generic;

namespace Skuld.Core.Models
{
    public class SkuldGuild
    {
        public ulong ID { get; set; }
        public string Prefix { get; set; }
        public ulong MutedRole { get; set; }
        public ulong[] JoinableRoles { get; set; }
        public ulong JoinRole { get; set; }
        public ulong UserJoinChannel { get; set; }
        public ulong UserLeaveChannel { get; set; }
        public string JoinMessage { get; set; }
        public string LeaveMessage { get; set; }
        public LevelNotification LevelNotification { get; set; }
        public string LevelUpMessage { get; set; }
        public ulong LevelUpChannel { get; set; }

        public List<CustomCommand> CustomCommands { get; set; }

        public GuildCommandModules Modules { get; set; }
        public GuildFeatureModules Features { get; set; }
        public List<GuildLevelRewards> LevelRewards { get; set; }
    }

    public class GuildCommandModules
    {
        public bool AccountsEnabled { get; set; }
        public bool ActionsEnabled { get; set; }
        public bool AdminEnabled { get; set; }
        public bool CustomEnabled { get; set; }
        public bool FunEnabled { get; set; }
        public bool InformationEnabled { get; set; }
        public bool LewdEnabled { get; set; }
        public bool SearchEnabled { get; set; }
        public bool StatsEnabled { get; set; }
        public bool WeebEnabled { get; set; }
    }

    public class GuildFeatureModules
    {
        public bool Pinning { get; set; }
        public bool Experience { get; set; }
    }

    public class GuildLevelRewards
    {
        public ulong RoleID { get; set; }
        public int LevelRequirement { get; set; }
    }
}