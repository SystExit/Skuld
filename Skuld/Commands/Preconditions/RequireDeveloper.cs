using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Skuld.Services;

namespace Skuld.Commands.Preconditions
{
    public class RequireDeveloper : PreconditionAttribute
    {
        public RequireDeveloper()
        {

        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (HostService.Configuration.Discord.Owners.Contains(context.User.Id) || (context.Client.GetApplicationInfoAsync().Result).Owner.Id == context.User.Id)
            { return Task.FromResult(PreconditionResult.FromSuccess()); }
            else
                return Task.FromResult(PreconditionResult.FromError("Not a bot owner/developer"));
        }
    }
}
