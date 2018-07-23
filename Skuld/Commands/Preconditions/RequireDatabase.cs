using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Skuld.Services;
using System;
using System.Threading.Tasks;

namespace Skuld.Commands.Preconditions
{
    public class RequireDatabase : PreconditionAttribute
    {
        public RequireDatabase()
        {
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var dbserv = HostService.Services.GetRequiredService<DatabaseService>();

            if (dbserv != null)
            {
                if (await dbserv.CheckConnectionAsync())
                    return PreconditionResult.FromSuccess();
            }

            return PreconditionResult.FromError("Command requires an active Database Connection");
        }
    }
}