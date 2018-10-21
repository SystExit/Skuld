using Discord.Commands;
using Skuld.Core.Models;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
{
    public class RequireTwitch : PreconditionAttribute
    {
        public RequireTwitch()
        {
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var perm = SkuldConfig.Load().Modules.TwitchModule;
            if (perm)
            { return Task.FromResult(PreconditionResult.FromSuccess()); }
            else
            { return Task.FromResult(PreconditionResult.FromError("Twitch Module Disabled")); }
        }
    }
}