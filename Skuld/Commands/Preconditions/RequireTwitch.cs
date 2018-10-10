using Discord.Commands;
using Skuld.Services;
using System;
using System.Threading.Tasks;

namespace Skuld.Commands.Preconditions
{
    public class RequireTwitch : PreconditionAttribute
    {
        public RequireTwitch()
        {
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var perm = HostService.Configuration.Modules.TwitchModule;
            if (perm)
            { return Task.FromResult(PreconditionResult.FromSuccess()); }
            else
            { return Task.FromResult(PreconditionResult.FromError("Twitch Module Disabled")); }
        }
    }
}