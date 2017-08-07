using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;

namespace Skuld.Models
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRolePrecondition : PreconditionAttribute
    {
        private AccessLevel Level;

        public RequireRolePrecondition(AccessLevel level)
        {
            Level = level;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            var access = GetPermission(context);            // Get the acccesslevel for this context

            if (access >= Level)                            // If the user's access level is greater than the required level, return success.
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("Insufficient permissions."));
        }

        public AccessLevel GetPermission(ICommandContext c)
        {
            if (c.User.IsBot)                                    // Prevent other bots from executing commands.
                return AccessLevel.Blocked;

            if (Tools.Config.Load().Owners.Contains(c.User.Id) || (Bot.bot.GetApplicationInfoAsync().Result).Owner.Id == c.User.Id) // Give configured owners special access.
                return AccessLevel.BotOwner;

            var user = c.User as IGuildUser;                // Check if the context is in a guild.
            if (user != null)
            {
                if (c.Guild.OwnerId == user.Id)                  // Check if the user is the guild owner.
                    return AccessLevel.ServerOwner;

                if (user.GuildPermissions.Administrator || user.GuildPermissions.ManageGuild)         // Check if the user has the administrator permission or Manage Guild role.
                    return AccessLevel.ServerAdmin;
                if (user.GuildPermissions.ManageMessages && user.GuildPermissions.BanMembers && user.GuildPermissions.KickMembers && user.GuildPermissions.ManageRoles)
                    return AccessLevel.ServerMod;
            }

            return AccessLevel.User;                             // If nothing else, return a default permission.
        }
    }
}
