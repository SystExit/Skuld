using System.Collections.Generic;

namespace Skuld.Core.Models.Skuld
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
}