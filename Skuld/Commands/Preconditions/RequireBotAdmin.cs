using Discord.Commands;
using Skuld.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Commands.Preconditions
{
    public class RequireBotAdmin : PreconditionAttribute
    {
        public RequireBotAdmin()
        {
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (HostService.Configuration.Discord.BotAdmins.Contains(context.User.Id) || (context.Client.GetApplicationInfoAsync().Result).Owner.Id == context.User.Id)
            { return Task.FromResult(PreconditionResult.FromSuccess()); }
            else
                return Task.FromResult(PreconditionResult.FromError("Not a bot owner/developer"));
        }
    }
}