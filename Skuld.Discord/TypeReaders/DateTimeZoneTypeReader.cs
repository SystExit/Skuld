using Discord.Commands;
using NodaTime;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.TypeReaders
{
    public class DateTimeZoneTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var result = DateTimeZoneProviders.Tzdb.GetZoneOrNull(input);

            if (result == null)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid TimeZone Input"));
            }
            else
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(result));
            }
        }
    }
}