using Discord.Commands;
using Skuld.Core.Generic.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
{
    public class RequireBotAdmin : PreconditionAttribute
    {
        public RequireBotAdmin()
        {
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (SkuldConfig.Load().Discord.BotAdmins.Contains(context.User.Id) || (context.Client.GetApplicationInfoAsync().Result).Owner.Id == context.User.Id)
            { return Task.FromResult(PreconditionResult.FromSuccess()); }
            else
                return Task.FromResult(PreconditionResult.FromError("Not a bot owner/developer"));
        }
    }
}