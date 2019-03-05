using Discord.Commands;
using Skuld.Core.Models;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.TypeReaders
{
    public class RoleConfigTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (GuildRoleConfig.FromString(input, context, out GuildRoleConfig config))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(config));
            }
            else
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid role configuration."));
            }
        }
    }
}