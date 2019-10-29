using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
{
    public class Disabled : PreconditionAttribute
    {
        public Disabled()
        {
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
            => Task.FromResult(PreconditionResult.FromError("Disabled"));
    }
}