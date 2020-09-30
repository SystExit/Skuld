using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Skuld.Bot.Discord.Preconditions
{
    public class RequireGuildVoiceChannelAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            bool isInChannel = (context.User as IGuildUser)?.VoiceChannel != null;

            if (isInChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"Not in a voice channel"));
        }
    }
}