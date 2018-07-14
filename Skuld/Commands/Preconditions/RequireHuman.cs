using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Skuld.Commands.Preconditions
{
    public class RequireHuman : PreconditionAttribute
    {
        public RequireHuman()
        {
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (!context.User.IsBot)
            { return Task.FromResult(PreconditionResult.FromSuccess()); }
            else
                return Task.FromResult(PreconditionResult.FromError("Bot's aren't supported"));
        }
    }
}