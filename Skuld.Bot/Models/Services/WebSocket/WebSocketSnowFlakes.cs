using System.Collections.Generic;

namespace Skuld.Bot.Models.Services.WebSocket
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