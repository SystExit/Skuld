using Discord.Commands;
using Discord.WebSocket;
using Skuld.Services;
using Skuld.Models.Database;

namespace Skuld.Commands
{
    public class SkuldCommandContext : ShardedCommandContext
    {
        public DiscordShardedClient Client { get; }
        public DatabaseService Database { get; }
        public SkuldUser DBUser { get; }
        public SkuldGuild DBGuild { get; }

        public SkuldCommandContext(DiscordShardedClient client, SocketUserMessage msg, DatabaseService database, SkuldUser user, SkuldGuild guild) : base(client, msg)
        {
            Client = client;
            Database = database;
            DBUser = user;
            DBGuild = guild;
        }
    }
}