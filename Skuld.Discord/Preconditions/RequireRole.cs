using Discord;
using Discord.Commands;
using Skuld.Discord.Models;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
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
            { return Task.FromResult(PreconditionResult.FromError("Insufficient permissions.")); }
        }

        public AccessLevel GetPermission(ICommandContext c)
        {
            if (c.User.IsBot)
                return AccessLevel.Blocked;

            IGuildUser user = (IGuildUser)c.User;
            if (user != null)
            {
                if (c.Guild.OwnerId == user.Id)
                    return AccessLevel.ServerOwner;
                if (user.GuildPermissions.Administrator)
                    return AccessLevel.ServerAdmin;
                if (user.GuildPermissions.ManageMessages && user.GuildPermissions.KickMembers && user.GuildPermissions.ManageRoles)
                    return AccessLevel.ServerMod;
            }

            return AccessLevel.User;
        }
    }
}