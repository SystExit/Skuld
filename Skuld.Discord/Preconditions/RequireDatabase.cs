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

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (Database.IsConnected)
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError("Command requires an active Database Connection"));
        }
    }
}