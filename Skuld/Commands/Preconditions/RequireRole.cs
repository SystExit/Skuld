using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Skuld.Core.Models.Discord;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Commands.Preconditions
{
    public class RequireRole : PreconditionAttribute
    {
        private readonly AccessLevel Level;
        private readonly DiscordShardedClient client = Services.HostService.Client;

        public RequireRole(AccessLevel level)
        {
            Level = level;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var access = GetPermission(context);
            if (access >= Level)
            { return Task.FromResult(PreconditionResult.FromSuccess()); }
            else
            { return Task.FromResult(PreconditionResult.FromError("Insufficient permissions.")); }
        }

        public AccessLevel GetPermission(ICommandContext c)
        {
            if (c.User.IsBot)
            { return AccessLevel.Blocked; }
            if (Services.HostService.Configuration.Discord.Owners.Contains(c.User.Id) || (client.GetApplicationInfoAsync().Result).Owner.Id == c.User.Id)
            { return AccessLevel.BotOwner; }
            IGuildUser user = (IGuildUser)c.User;
            if (user != null)
            {
                if (c.Guild.OwnerId == user.Id)
                { return AccessLevel.ServerOwner; }
                if (user.GuildPermissions.Administrator)
                { return AccessLevel.ServerAdmin; }
                if (user.GuildPermissions.ManageMessages && user.GuildPermissions.BanMembers && user.GuildPermissions.KickMembers && user.GuildPermissions.ManageRoles)
                { return AccessLevel.ServerMod; }
            }

            return AccessLevel.User;                             // If nothing else, return a default permission.
        }
    }
}