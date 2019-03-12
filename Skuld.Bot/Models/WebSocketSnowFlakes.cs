using System;
using System.Collections.Generic;
using System.Text;

namespace Skuld.Bot.Models
{
    public class WebSocketSnowFlakes
    {
        public ulong GuildID;
        public string Type;
        public List<WebSocketSnowFlake> Data;
    }
    public class WebSocketSnowFlake
    {
        public string Name;
        public ulong ID;
    }
}
