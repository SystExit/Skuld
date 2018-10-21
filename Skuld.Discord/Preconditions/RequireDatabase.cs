using Discord.Commands;
using Skuld.Database;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
{
    public class RequireDatabase : PreconditionAttribute
    {
        public RequireDatabase()
        {
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (await DatabaseClient.CheckConnectionAsync())
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("Command requires an active Database Connection");
        }
    }
}