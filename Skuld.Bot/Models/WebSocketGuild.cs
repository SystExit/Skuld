﻿namespace Skuld.Bot.Models.WebSocket
{
    public struct WebSocketGuild
    {
        public string Name { get; internal set; }
        public string GuildIconUrl { get; internal set; }
        public ulong Id { get; internal set; }
    }
}