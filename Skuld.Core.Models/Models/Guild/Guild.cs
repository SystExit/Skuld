namespace Skuld.Core.Models
{
    public class Guild
    {
        public ulong Id { get; set; }
        public string JoinMessage { get; set; }
        public string LeaveMessage { get; set; }
        public ulong JoinRole { get; set; }
        public string Prefix { get; set; }
        public ulong MutedRole { get; set; }
        public ulong JoinChannel { get; set; }
        public ulong LeaveChannel { get; set; }
        public ulong LevelUpChannel { get; set; }
        public string LevelUpMessage { get; set; }
        public LevelNotification LevelNotification { get; set; }
        public string MoneyName { get; set; }
        public string MoneyIcon { get; set; }
    }
}