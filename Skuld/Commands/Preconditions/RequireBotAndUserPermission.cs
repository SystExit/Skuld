using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Skuld.Commands.Preconditions
{
    public class RequireBotAndUserPermission : PreconditionAttribute
    {
        private readonly GuildPermission Permission;

        public RequireBotAndUserPermission(GuildPermission perm)
        {
            Permission = perm;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var perm = GetPermission(context);
            if (perm)
            { return Task.FromResult(PreconditionResult.FromSuccess()); }
            else
            { return Task.FromResult(PreconditionResult.FromError("Insufficient permissions.")); }
        }

        public bool GetPermission(ICommandContext c)
        {
            if (c.User.IsBot)
            { return false; }

            IGuildUser user = (IGuildUser)c.User;
            IGuildUser botu = c.Guild.GetCurrentUserAsync().Result;

            foreach (var perm in user.GuildPermissions.ToList())
            {
                foreach (var bperm in botu.GuildPermissions.ToList())
                {
                    if (perm == Permission && bperm == Permission)
                        return true;
                }
            }

            return false;
        }
    }
}