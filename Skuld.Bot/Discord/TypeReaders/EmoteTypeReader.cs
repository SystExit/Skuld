using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Skuld.Bot.Discord.TypeReaders
{
    public class EmoteTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (Emote.TryParse(input, out Emote emote))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(emote));
            }
            else
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid Emoji"));
            }
        }
    }
}