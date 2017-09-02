using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Models
{
    public class Starboard
    {
        public ulong GuildID { get; set; }
        public ulong ChannelID { get; set; }
        public ulong MessageID { get; set; }
        public int Stars { get; set; }
        public string DateAdded { get; set; }
        public bool Locked { get; set; }
    }
}
