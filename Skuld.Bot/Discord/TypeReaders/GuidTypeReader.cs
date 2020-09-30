using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Skuld.Bot.Discord.TypeReaders
{
    public class GuidTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (Guid.TryParse(input, out Guid guid))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(guid));
            }
            else
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid guid"));
            }
        }
    }
}