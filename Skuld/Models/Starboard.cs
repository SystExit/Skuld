namespace Skuld.Models
{
    public class Starboard
    {
        public ulong GuildID { get; set; }
        public ulong ChannelID { get; set; }
        public ulong MessageID { get; set; }
        public ulong OriginalMessageID { get; set; }
        public int Stars { get; set; }
        public string DateAdded { get; set; }
    }
}
