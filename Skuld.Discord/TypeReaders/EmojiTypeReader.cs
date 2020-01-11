using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.TypeReaders
{
    public class EmojiTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var unicode = EmojiOne.EmojiOne.ShortnameToUnicode(input);
            if (unicode != null)
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(new Emoji(unicode)));
            }
            else
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid Emoji"));
            }
        }
    }
}