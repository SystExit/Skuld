using Discord.Commands;
using Skuld.Core.Models.Skuld;
using Skuld.Database;
using Skuld.Discord.Utilities;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
{
    public class RequireEnabledModule : PreconditionAttribute
    {
        public RequireEnabledModule()
        {

        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Guild == null) return PreconditionResult.FromSuccess();

            var gld = await DatabaseClient.GetGuildAsync(context.Guild.Id);

            if(gld.Successful)
            {
                var guild = gld.Data as SkuldGuild;

                if (MessageTools.ModuleDisabled(guild.Modules, command))
                    return PreconditionResult.FromError($"The module: `{command.Module.Name}` is disabled, contact a server administrator to enable it");
            }

            return PreconditionResult.FromSuccess();
        }
    }
}