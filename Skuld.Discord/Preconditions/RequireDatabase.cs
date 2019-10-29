using Discord.Commands;
using Skuld.Core.Models;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
{
    public class RequireDatabaseAttribute : PreconditionAttribute
    {
        public RequireDatabaseAttribute()
        {
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (Database.IsConnected)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("Command requires an active Database Connection");
        }
    }
}