using System.Collections.Generic;

namespace Skuld.Core.Models
{
    public class UserExperience
    {
        public ulong UserID { get; set; }
        public List<GuildExperience> GuildExperiences { get; set; }

        public UserExperience()
        {
            GuildExperiences = new List<GuildExperience>();
        }
    }

    public class GuildExperience
    {
        public ulong GuildID { get; set; }
        public ulong Level { get; set; }
        public ulong XP { get; set; }
        public ulong TotalXP { get; set; }
        public ulong LastGranted { get; set; }
    }
}