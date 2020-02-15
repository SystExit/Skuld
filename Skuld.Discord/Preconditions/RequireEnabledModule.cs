using Discord.Commands;
using Skuld.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
{
    public class RequireEnabledModuleAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild == null) return Task.FromResult(PreconditionResult.FromSuccess());

            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (Database.Modules.ToList().FirstOrDefault(x => x.Id == context.Guild.Id).ModuleDisabled(command))
            {
                return Task.FromResult(PreconditionResult.FromError($"The module: `{command.Module.Name}` is disabled, contact a server administrator to enable it"));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}