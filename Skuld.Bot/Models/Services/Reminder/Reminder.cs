using Discord;
using System;

namespace Skuld.Bot.Models.Services.Reminder
{
    public struct Reminder
    {
        public ushort Id;
        public IUser User;
        public IMessageChannel Channel;
        public ulong Timeout;
        public string Content;
        public DateTime Created;
    }
}