namespace Skuld.Core.Models
{
    public class Reputation
    {
        public int Id { get; set; }

        /// <summary>
        /// The Person Recieving The Reputation
        /// </summary>
        public ulong Repee { get; set; }

        /// <summary>
        /// The Person Giving The Reputation
        /// </summary>
        public ulong Reper { get; set; }

        public ulong Timestamp { get; set; }
    }
}