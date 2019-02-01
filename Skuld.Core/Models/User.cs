using System.Collections.Generic;

namespace Skuld.Core.Models
{
    public class SkuldUser
    {
        public ulong ID { get; set; }
        public bool Banned { get; set; }
        public string Description { get; set; }
        public bool CanDM { get; set; }
        public ulong Money { get; set; }
        public string Language { get; set; }
        public uint HP { get; set; }
        public uint Patted { get; set; }
        public uint Pats { get; set; }
        public ulong Daily { get; set; }
        public string AvatarUrl { get; set; }
        public bool RecurringBlock { get; set; }
        public bool UnlockedCustBG { get; set; }
        public string Background { get; set; }

        public List<GuildExperience> GuildExperience { get; set; }
        public List<int> UpvotedPastas { get; set; }
        public List<int> DownvotedPastas { get; set; }
        public List<CommandUsage> CommandUsage { get; set; }
    }
}