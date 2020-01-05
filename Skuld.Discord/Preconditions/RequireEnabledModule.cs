using Discord.Commands;
using Skuld.Core.Models;
using Skuld.Discord.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
{
    public class RequireEnabledModuleAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();

            if (MessageTools.ModuleDisabled(Database.Modules.FirstOrDefault(x => x.Id == context.Guild.Id), command))
            {
                return Task.FromResult(PreconditionResult.FromError($"The module: `{command.Module.Name}` is disabled, contact a server administrator to enable it"));
            }

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}