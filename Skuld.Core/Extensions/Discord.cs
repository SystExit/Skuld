using System;
using Discord;
using Discord.WebSocket;
using Skuld.Core.Models;
using Skuld.Core.Utilities;

namespace Skuld.Core.Extensions
{
    public static class Discord
    {
        public static Embed GetWhois(this IGuildUser user, Color EmbedColor, DiscordShardedClient Client, SkuldConfig Configuration)
        {
            string nickname = null;
            string status = "";

            if (!String.IsNullOrEmpty(user.Nickname)) nickname = $"({user.Nickname})";
            if (user.Status == UserStatus.Online) status = DiscordEmotes.Online + " Online";
            if (user.Status == UserStatus.AFK || user.Status == UserStatus.Idle) status = DiscordEmotes.Idle + " AFK/Idle";
            if (user.Status == UserStatus.DoNotDisturb) status = DiscordEmotes.DoNotDisturb + " Do not Disturb/Busy";
            if (user.Status == UserStatus.Invisible) status = DiscordEmotes.Invisible + " Invisible";
            if (user.Status == UserStatus.Offline) status = DiscordEmotes.Offline + " Offline";
            if (user.Activity != null) if (user.Activity.Type == ActivityType.Streaming) status = DiscordEmotes.Streaming + $" {user.Activity.Type.ToString()}";

            string pngavatar = user.GetAvatarUrl(ImageFormat.Png);
            string gifavatar = user.GetAvatarUrl(ImageFormat.Gif);
            string name = user.Username + $"#{user.DiscriminatorValue}";

            if (user.Nickname != null) name += $" {nickname}";

            var embed = new EmbedBuilder
            {
                Color = EmbedColor,
                Author = new EmbedAuthorBuilder { IconUrl = pngavatar ?? "", Name = name },
                ImageUrl = user.GetAvatarUrl()
            };

            int seencount = 0;
            foreach (var item in Client.Guilds)
            {
                if (item.GetUser(user.Id) != null) seencount++;
            }

            string game = null;

            if (user.Activity != null) game = user.Activity.Name;
            else game = "Nothing";

            embed.AddField(":id: ID", user.Id.ToString() ?? "Unknown", true);
            embed.AddField(":vertical_traffic_light: Status", status ?? "Unknown", true);
            embed.AddField(":video_game: Currently Playing", game, true);
            embed.AddField(":robot: Bot?", user.IsBot.ToString() ?? "Unknown", true);
            embed.AddField(":eyes: Mutual Servers", $"{seencount} servers", true);
            embed.AddField(":shield: Roles", $"[{user.RoleIds.Count}] Do `{Configuration.Discord.Prefix}roles` to see your roles");

            var createdatstring = user.CreatedAt.GetStringfromOffset(DateTime.UtcNow);
            embed.AddField(":globe_with_meridians: Discord Join", user.CreatedAt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + $" ({createdatstring})\t`DD/MM/YYYY`");

            if (user.JoinedAt.HasValue)
            {
                var joinedatstring = user.JoinedAt.Value.GetStringfromOffset(DateTime.UtcNow);
                embed.AddField(":inbox_tray: Server Join", user.JoinedAt.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + $" ({joinedatstring})\t`DD/MM/YYYY`");
            }
            else
            {
                embed.AddField(":inbox_tray: Server Join", "N/A");
            }

            return embed.Build();
        }
    }
}
