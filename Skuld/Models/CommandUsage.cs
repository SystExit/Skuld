namespace Skuld.Models
{
    public class CommandUsage
    {
        public ulong ID { get; set; }
        public ulong UserID { get; set; }
        public ulong UserUsage { get; set; }
        public string Command { get; set; }
    }
}
