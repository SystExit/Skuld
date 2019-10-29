namespace Skuld.Core.Models
{
    public class CustomCommand
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
    }
}