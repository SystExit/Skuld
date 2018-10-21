using Discord;
using Discord.WebSocket;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using System;
using System.Linq;
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

        public static async Task<Embed> GetSummaryAsync(this IGuild guild, DiscordShardedClient client, SkuldGuild skuldguild = null)
        {
            var embed = new EmbedBuilder();
            var botusers = await guild.RobotMembersAsync();
            var humanusers = await guild.HumanMembersAsync();
            var ratio = await guild.GetBotUserRatioAsync();
            var afkchan = await guild.GetAFKChannelAsync();
            var owner = await guild.GetOwnerAsync();
            var userratio = await guild.GetBotUserRatioAsync();
            var channels = await guild.GetChannelsAsync();
            var afktimeout = ((guild.AFKTimeout % 3600) / 60);

            embed.WithTitle(guild.Name);

            embed.WithThumbnailUrl(guild.IconUrl ?? "");

            embed.WithColor(EmbedUtils.RandomColor());

            string afkname = null;
            if (afkchan != null)
            {
                afkname = afkchan.Name;
            }
            else
            {
                afkname = "";
            }

            embed.AddInlineField("Users", $"Users: {humanusers.ToString("N0")}\nBots: {botusers.ToString("N0")}\nRatio: {ratio}%");
            embed.AddInlineField("Shard", client.GetShardIdFor(guild));
            embed.AddInlineField("Verification Level", guild.VerificationLevel);
            embed.AddInlineField("Voice Region", guild.VoiceRegionId);
            embed.AddInlineField("Owner", $"{owner.Username}#{owner.DiscriminatorValue}");
            embed.AddInlineField("Text Channels", channels.Count(x => x.GetType() == typeof(SocketTextChannel)));
            embed.AddInlineField("Voice Channels", channels.Count(x => x.GetType() == typeof(SocketVoiceChannel)));
            embed.AddInlineField("AFK Timeout", afktimeout + " minutes");
            embed.AddInlineField("Default Notifications", guild.DefaultMessageNotifications);
            embed.AddInlineField("Created", guild.CreatedAt.ToString("dd'/'MM'/'yyyy HH:mm:ss") + "\t(DD/MM/YYYY)");
            if (skuldguild != null)
            {
                embed.AddInlineField($"Emotes [{guild.Emotes.Count}]", $" Use `{skuldguild.Prefix}server-emojis` to view them");
                embed.AddInlineField($"Roles [{guild.Roles.Count}]", $" Use `{skuldguild.Prefix}server-roles` to view them");
            }
            else
            {
                var conf = SkuldConfig.Load();
                embed.AddInlineField($"Emotes [{guild.Emotes.Count}]", $" Use `{conf.Discord.Prefix}server-emojis` to view them");
                embed.AddInlineField($"Roles [{guild.Roles.Count}]", $" Use `{conf.Discord.Prefix}server-roles` to view them");
            }

            return embed.Build();
        }

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
            await GenericLogger.AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Deleted a timed message", LogSeverity.Info));
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

        public static Embed GetWhois(this IGuildUser user, Color EmbedColor, DiscordShardedClient Client, SkuldConfig Configuration)
        {
            string nickname = null;
            string status = "";

            if (!string.IsNullOrEmpty(user.Nickname)) nickname = $"({user.Nickname})";
            if (user.Status == UserStatus.Online) status = DiscordTools.Online_Emote + " Online";
            if (user.Status == UserStatus.AFK || user.Status == UserStatus.Idle) status = DiscordTools.Idle_Emote + " AFK/Idle";
            if (user.Status == UserStatus.DoNotDisturb) status = DiscordTools.DoNotDisturb_Emote + " Do not Disturb/Busy";
            if (user.Status == UserStatus.Invisible) status = DiscordTools.Invisible_Emote + " Invisible";
            if (user.Status == UserStatus.Offline) status = DiscordTools.Offline_Emote + " Offline";
            if (user.Activity != null) if (user.Activity.Type == ActivityType.Streaming) status = DiscordTools.Streaming_Emote + $" {user.Activity.Type.ToString()}";

            string name = user.Username + $"#{user.DiscriminatorValue}";

            if (user.Nickname != null) name += $" {nickname}";

            var embed = new EmbedBuilder
            {
                Color = EmbedColor,
                Author = new EmbedAuthorBuilder { IconUrl = user.GetAvatarUrl(ImageFormat.Png), Name = name },
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
        public static Color FromHex(this string hex)
        {
            var col = System.Drawing.ColorTranslator.FromHtml(hex);
            return new Color(col.R, col.G, col.B);
        }
    }
}