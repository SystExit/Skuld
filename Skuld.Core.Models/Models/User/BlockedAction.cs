using System.ComponentModel.DataAnnotations;

namespace Skuld.Core.Models
{
    public class BlockedAction
    {
        [Key]
        public ulong Id { get; set; }

        public ulong Blocker { get; set; }
        public ulong Blockee { get; set; }
    }
}