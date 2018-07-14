namespace Skuld.Models.Database
{
    public class CommandUsage
    {
        public ulong UserID { get; set; }
        public string Command { get; set; }
        public ulong Usage { get; set; }
    }
}