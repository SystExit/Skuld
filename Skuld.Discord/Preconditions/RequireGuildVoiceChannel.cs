using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Skuld.Discord.Preconditions
{
    public class RequireGuildVoiceChannelAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            bool isInChannel = (context.User as IGuildUser)?.VoiceChannel != null;

            if (isInChannel)
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError($"Not in a voice channel");
        }
    }
}