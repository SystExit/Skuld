using Discord.Commands;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Skuld.Discord.TypeReaders
{
    public class IPAddressTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            if (IPAddress.TryParse(input, out IPAddress iPAddress))
            {
                return Task.FromResult(TypeReaderResult.FromSuccess(iPAddress));
            }
            else
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid ip address"));
            }
        }
    }
}