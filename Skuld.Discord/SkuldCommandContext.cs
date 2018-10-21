using Discord.Commands;
using Discord.WebSocket;
using Skuld.Core.Models;

namespace Skuld.Discord
{
    public class SkuldCommandContext : ShardedCommandContext
    {
        public SkuldUser DBUser { get; }
        public SkuldGuild DBGuild { get; }

        public SkuldCommandContext(DiscordShardedClient client, SocketUserMessage msg, SkuldUser user, SkuldGuild guild) : base(client, msg)
        {
            DBUser = user;
            DBGuild = guild;
        }
    }
}