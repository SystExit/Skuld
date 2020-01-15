namespace Skuld.Core.Models
{
    public class Issue
    {
        public ulong Id { get; set; }
        public ulong IssueChannelMessageId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public ulong SubmitterId { get; set; }
        public bool HasSent { get; set; } = false;
    }
}
