using System;
using System.Collections.Generic;
using System.Text;

namespace Skuld.Models
{
    public class CustomCommand
    {
        public ulong GuildID { get => guildid; }
        public string Command { get => command; }
        public string Content { get => content; }
        ulong guildid;
        string command;
        string content;
    }
}
