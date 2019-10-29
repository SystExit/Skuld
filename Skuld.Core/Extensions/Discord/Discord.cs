using Discord;
using Discord.WebSocket;
using Skuld.Core.Generic.Models;
using Skuld.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Core.Extensions
{
    public static class DiscordExtensions
    {
        public static void AddInlineField(this EmbedBuilder embed, string name, object value)
            => embed.AddField(name, value, true);

        public static async Task<int> RobotMembersAsync(this IGuild guild)
        {
            await guild.DownloadUsersAsync();

            return (await guild.GetUsersAsync(CacheMode.AllowDownload)).Count(x => x.IsBot);
        }

        public static async Task<int> HumanMembersAsync(this IGuild guild)
        {
            await guild.DownloadUsersAsync();

            return (await guild.GetUsersAsync(CacheMode.AllowDownload)).Count(x => !x.IsBot);
        }

        public static async Task<decimal> GetBotUserRatioAsync(this IGuild guild)
        {
            await guild.DownloadUsersAsync();
            var botusers = await guild.RobotMembersAsync();
            var userslist = await guild.GetUsersAsync();
            var users = userslist.Count;
            return Math.Round((((decimal)botusers / users) * 100m), 2);
        }

        public static Color GetHighestRoleColor(this IGuildUser user, IGuild guild)
        {
            IRole highestRole = null;

            foreach (var roleid in user.RoleIds.OrderByDescending(x => x))
            {
                var role = guild.GetRole(roleid);

                if (role.Color == Color.Default)
                {
                    continue;
                }

                highestRole = role;
                break;
            }

            return (highestRole != null ? highestRole.Color : Color.Default);
        }

        public static string ToEmoji(this ClientType client)
         => client switch
         {
             ClientType.Desktop => "🖥️",
             ClientType.Mobile => "📱",
             ClientType.Web => "🔗",

             _ => "🖥️",
         };

        public static string ToMessage(this Embed embed)
        {
            string message = "";
            if (embed.Author.HasValue)
                message += $"**__{embed.Author.Value.Name}__**\n";
            if (!string.IsNullOrEmpty(embed.Title))
                message += $"**{embed.Title}**\n";
            if (!string.IsNullOrEmpty(embed.Description))
                message += embed.Description + "\n";
            foreach (var field in embed.Fields)
                message += $"__{field.Name}__\n{field.Value}\n\n";
            if (embed.Video.HasValue)
                message += embed.Video.Value.Url + "\n";
            if (embed.Thumbnail.HasValue)
                message += embed.Thumbnail.Value.Url + "\n";
            if (embed.Image.HasValue)
                message += embed.Image.Value.Url + "\n";
            if (embed.Footer.HasValue)
                message += $"`{embed.Footer.Value.Text}`";
            if (embed.Timestamp.HasValue)
                message += " | " + embed.Timestamp.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss tt");
            return message;
        }

        public static async Task DeleteAfterSecondsAsync(this IUserMessage message, int timeout)
        {
            await Task.Delay((timeout * 1000));
            await message.DeleteAsync();
            //await GenericLogger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Deleted a timed message", LogSeverity.Info));
        }

        public static async Task<bool> CanEmbedAsync(this IMessageChannel channel, IGuild guild = null)
        {
            if (guild == null) return true;
            else
            {
                var curr = await guild.GetCurrentUserAsync();
                var chan = await guild.GetChannelAsync(channel.Id);
                var perms = curr.GetPermissions(chan);
                return perms.EmbedLinks;
            }
        }

        public static Embed GetWhois(this IUser user, IGuildUser guildUser, IReadOnlyCollection<ulong> roles, Color EmbedColor, DiscordShardedClient Client, SkuldConfig Configuration)
        {
            string status = "";

            if (user.Activity != null)
            {
                if (user.Activity.Type == ActivityType.Streaming) status = DiscordTools.Streaming_Emote;
                else status = user.Status.StatusToEmote();
            }
            else status = user.Status.StatusToEmote();

            var embed = new EmbedBuilder
            {
                Color = EmbedColor,
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatarUrl(ImageFormat.Png),
                    Name = $"{user.Username}{(guildUser != null ? (guildUser.Nickname != null ? $" ({guildUser.Nickname})" : "") : "")}#{user.DiscriminatorValue}"
                },
                ThumbnailUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
            };

            embed.AddInlineField(":id:", user.Id.ToString() ?? "Unknown");
            embed.AddInlineField(":vertical_traffic_light:", status ?? "Unknown");

            if (user.Activity != null)
            {
                embed.AddInlineField(":video_game:", user.Activity.ActivityToString());
            }

            embed.AddInlineField("🤖", user.IsBot ? "✔️" : "❌");

            embed.AddInlineField("Mutual Servers", $"{Client.GetUser(user.Id).MutualGuilds.Count()}");

            StringBuilder clientString = new StringBuilder();
            foreach (var client in user.ActiveClients)
            {
                clientString = clientString.Append(client.ToEmoji());

                if (user.ActiveClients.Count > 1 && client != user.ActiveClients.Last())
                    clientString.Append(", ");
            }

            embed.AddInlineField($"Active Client{(user.ActiveClients.Count > 1 ? "s" : "")}", $"{clientString}");

            if (roles != null)
            {
                embed.AddField(":shield: Roles", $"[{roles.Count}] Do `{Configuration.Discord.Prefix}roles` to see your roles");
            }

            var createdatstring = user.CreatedAt.GetStringfromOffset(DateTime.UtcNow);
            embed.AddField(":globe_with_meridians: Discord Join", user.CreatedAt.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + $" ({createdatstring})\t`DD/MM/YYYY`");

            if (guildUser != null)
            {
                var joinedatstring = guildUser.JoinedAt.Value.GetStringfromOffset(DateTime.UtcNow);
                embed.AddField(":inbox_tray: Server Join", guildUser.JoinedAt.Value.ToString("dd'/'MM'/'yyyy hh:mm:ss tt") + $" ({joinedatstring})\t`DD/MM/YYYY`");
            }

            if (guildUser.PremiumSince.HasValue)
            {
                var icon = guildUser.PremiumSince.Value.BoostMonthToEmote();

                var offsetString = guildUser.PremiumSince.Value.GetStringfromOffset(DateTime.UtcNow);

                embed.AddField(DiscordTools.NitroBoostEmote + " Boosting Since", $"{(icon == null ? "" : icon + " ")}{guildUser.PremiumSince.Value.UtcDateTime.ToString("dd'/'MM'/'yyyy hh:mm:ss tt")} ({offsetString})\t`DD/MM/YYYY`");
            }

            return embed.Build();
        }

        public static Color FromHex(this string hex)
        {
            var col = System.Drawing.ColorTranslator.FromHtml(hex);
            return new Color(col.R, col.G, col.B);
        }

        public static string HexFromStatus(this UserStatus status)
        {
            switch (status)
            {
                case UserStatus.Online:
                    return "#43b581";

                case UserStatus.AFK:
                case UserStatus.Idle:
                    return "#faa61a";

                case UserStatus.DoNotDisturb:
                    return "#f04747";

                case UserStatus.Invisible:
                case UserStatus.Offline:
                    return "#747f8d";

                default:
                    return "#fff";
            }
        }

        public static string ActivityToString(this IActivity activity)
        {
            StringBuilder builder = new StringBuilder();

            switch (activity.Type)
            {
                case ActivityType.Listening:
                    builder = builder.Append("Listening to");
                    break;

                case ActivityType.Playing:
                    builder = builder.Append("Playing");
                    break;

                case ActivityType.Streaming:
                    builder = builder.Append("Streaming");
                    break;

                case ActivityType.Watching:
                    builder = builder.Append("Watching");
                    break;
            }

            builder = builder.Append(" ");
            builder = builder.Append(activity.Name);

            return builder.ToString();
        }

        public static string StatusToEmote(this UserStatus status) => status switch
        {
            UserStatus.Online => DiscordTools.Online_Emote,
            UserStatus.AFK => DiscordTools.Idle_Emote,
            UserStatus.Idle => DiscordTools.Idle_Emote,
            UserStatus.DoNotDisturb => DiscordTools.DoNotDisturb_Emote,
            UserStatus.Invisible => DiscordTools.Offline_Emote,
            UserStatus.Offline => DiscordTools.Offline_Emote,

            _ => DiscordTools.Offline_Emote,
        };

        public static string BoostMonthToEmote(this DateTimeOffset boostMonths)
        {
            var months = DiscordTools.MonthsBetween(DateTime.UtcNow, boostMonths.Date);

            if (months <= 1)
            {
                return DiscordTools.NitroBoostRank1Emote;
            }
            if (months == 2)
            {
                return DiscordTools.NitroBoostRank2Emote;
            }
            if (months == 3)
            {
                return DiscordTools.NitroBoostRank3Emote;
            }
            if (months >= 4)
            {
                return DiscordTools.NitroBoostRank4Emote;
            }

            return null;
        }
    }
}