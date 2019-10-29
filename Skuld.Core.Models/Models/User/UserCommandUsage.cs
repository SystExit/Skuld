namespace Skuld.Core.Models
{
    public class UserCommandUsage
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public ulong Usage { get; set; }
        public string Command { get; set; }
    }
}