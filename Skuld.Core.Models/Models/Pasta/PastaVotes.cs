namespace Skuld.Core.Models
{
    public class PastaVotes
    {
        public ulong Id { get; set; }
        public ulong PastaId { get; set; }
        public ulong VoterId { get; set; }
        public bool Upvote { get; set; }
    }
}