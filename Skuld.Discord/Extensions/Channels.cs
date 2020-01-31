using Discord;

namespace Skuld.Discord.Extensions
{
    public static class ChannelExtensions
    {
        public static string JumpLink(this IGuildChannel channel)
            => $"https://discordapp.com/channels/{channel.GuildId}/{channel.Id}";
    }
}