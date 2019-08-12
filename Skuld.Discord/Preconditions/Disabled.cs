using Discord.Commands;
using Skuld.Core.Models.Skuld;
using Skuld.Database;
using Skuld.Discord.Utilities;
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