using Discord;
using Discord.Commands;
using Skuld.Core.Utilities;
using Skuld.Bot.Discord.Models;
using System;
using System.Threading.Tasks;

namespace Skuld.Bot.Discord.Preconditions
{
    public class RequireRole : PreconditionAttribute
    {
        private readonly AccessLevel Level;

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
            { return Task.FromResult(PreconditionResult.FromError("Insufficient permissions")); }
        }

        public AccessLevel GetPermission(ICommandContext c)
        {
            if (c.User.IsBot)
                return AccessLevel.Blocked;

            if(c.User is IGuildUser user)
            {
                if (c.Guild.OwnerId == user.Id)
                    return AccessLevel.ServerOwner;
                if (user.GuildPermissions.Administrator)
                    return AccessLevel.ServerAdmin;
                if (user.GuildPermissions.RawValue >= DiscordUtilities.ModeratorPermissions.RawValue)
                    return AccessLevel.ServerMod;
            }            

            return AccessLevel.User;
        }
    }
}