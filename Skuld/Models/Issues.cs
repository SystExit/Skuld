namespace Skuld.Models
{
    public class Issues
    {
        public uint ID { get; set; }
        public ulong GuildID { get; set; }
        public ulong UserID { get; set; }
        public ulong MessageID { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public string Content { get; set; }
    }
}
