using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Skuld.Core.Services;
using Skuld.Models.Database;
using Skuld.Services;
using Skuld.Utilities.Discord;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Skuld.Extensions
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
                var mservconf = HostService.Services.GetRequiredService<MessageService>().config;
                embed.AddInlineField($"Emotes [{guild.Emotes.Count}]", $" Use `{mservconf.Prefix}server-emojis` to view them");
                embed.AddInlineField($"Roles [{guild.Roles.Count}]", $" Use `{mservconf.Prefix}server-roles` to view them");
            }

            return embed.Build();
        }

        public static string ToString(this Embed embed)
        {
            string message = "";

            return message;
        }

        public static string ToMessage(this Embed embed)
        {
            string message = "";
            if (embed.Author.HasValue)
                message += $"**__{embed.Author.Value.Name}__**\n";
            if (!String.IsNullOrEmpty(embed.Title))
                message += $"**{embed.Title}**\n";
            if (!String.IsNullOrEmpty(embed.Description))
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
            await HostService.Services.GetRequiredService<GenericLogger>().AddToLogsAsync(new Core.Models.LogMessage("MsgDisp", $"Deleted a timed message", LogSeverity.Info));
        }

        public static async Task<bool> CanEmbedAsync(this IMessageChannel channel, IGuild guild = null)
        {
            if (guild == null) return true;
            else return (await guild.GetCurrentUserAsync()).GetPermissions(channel as IGuildChannel).EmbedLinks;
        }

        public static string ToText(this Embed embed)
        {
            string message = "";
            if (embed.Author.HasValue)
                message += $"**__{embed.Author.Value.Name}__**\n";
            if (!String.IsNullOrEmpty(embed.Title))
                message += $"**{embed.Title}**\n";
            if (!String.IsNullOrEmpty(embed.Description))
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
    }
}