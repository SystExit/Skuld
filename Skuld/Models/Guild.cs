namespace Skuld.Models
{
    public class SkuldGuild
    {
        public ulong ID { get; set; }
        public string Name { get; set; }
        public string JoinMessage { get; set; }
        public string LeaveMessage { get; set; }
        public ulong LogChannel { get; set; }
        public ulong AutoJoinRole { get; set; }
        public string[] DisabledCommands { get; set; }
        public string[] DisabledModules { get; set; }
        public string Prefix { get; set; }
        public uint Users { get; set; }
        public bool LogEnabled { get; set; }
        public string[] JoinableRoles { get; set; }
        public ulong TwitchNotifChannel { get; set; }
        public ulong TwitterLogChannel { get; set; }
    }
}
