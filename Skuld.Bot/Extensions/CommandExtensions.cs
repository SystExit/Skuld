using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Skuld.Core;
using Skuld.Core.Models;
using Skuld.Discord.Extensions;
using SysEx.Net.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Bot.Extensions
{
    public static class CommandExtensions
    {
        public static async Task<Embed> GetSummaryAsync(this IGuild guild, DiscordShardedClient client, ICommandContext context, Guild skuldguild = null)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var config = Database.Configurations.FirstOrDefault(x => x.Id == SkuldAppContext.ConfigurationId);

            var botusers = await guild.RobotMembersAsync().ConfigureAwait(false);
            var humanusers = await guild.HumanMembersAsync().ConfigureAwait(false);
            var ratio = await guild.GetBotUserRatioAsync().ConfigureAwait(false);
            var afkchan = await guild.GetAFKChannelAsync().ConfigureAwait(false);
            var owner = await guild.GetOwnerAsync().ConfigureAwait(false);
            var userratio = await guild.GetBotUserRatioAsync().ConfigureAwait(false);
            var channels = await guild.GetChannelsAsync().ConfigureAwait(false);
            var afktimeout = guild.AFKTimeout % 3600 / 60;

            var embed = new EmbedBuilder()
                .AddFooter(context)
                .WithTitle(guild.Name)
                .AddAuthor(context.Client)
                .WithColor(EmbedExtensions.RandomEmbedColor())
                .AddInlineField("Users", $"Users: {humanusers.ToString("N0", null)}\nBots: {botusers.ToString("N0", null)}\nRatio: {ratio}%")
                .AddInlineField("Shard", client?.GetShardIdFor(guild))
                .AddInlineField("Verification Level", guild.VerificationLevel)
                .AddInlineField("Voice Region", guild.VoiceRegionId)
                .AddInlineField("Owner", $"{owner.Username}#{owner.DiscriminatorValue}")
                .AddInlineField("Text Channels", channels.Count(x => x.GetType() == typeof(SocketTextChannel)))
                .AddInlineField("Voice Channels", channels.Count(x => x.GetType() == typeof(SocketVoiceChannel)))
                .AddInlineField("AFK Timeout", afktimeout + " minutes")
                .AddInlineField("Default Notifications", guild.DefaultMessageNotifications)
                .AddInlineField("Created", guild.CreatedAt.ToString("dd'/'MM'/'yyyy HH:mm:ss", null) + "\t(DD/MM/YYYY)")
                .AddInlineField($"Emotes [{guild.Emotes.Count}]", $" Use `{skuldguild?.Prefix ?? config.Prefix}server-emojis` to view them")
                .AddInlineField($"Roles [{guild.Roles.Count}]", $" Use `{skuldguild?.Prefix ?? config.Prefix}server-roles` to view them");

            if (!string.IsNullOrEmpty(afkchan?.Name)) embed.AddInlineField("AFK Channel", $"[#{afkchan.Name}]({afkchan.JumpLink()})");

            if (!string.IsNullOrEmpty(guild.IconUrl)) embed.WithThumbnailUrl(guild.IconUrl);

            return embed.Build();
        }

        #region Pagination

        public static IList<string> PaginateList(this IReadOnlyList<MemeEndpoints> list, int pagehoist = 10)
        {
            var pages = new List<string>();
            string pagetext = "";

            for (int x = 0; x < list.Count; x++)
            {
                var obj = list[x];

                pagetext += $"Template: {obj.Name} | Required Sources: {obj.RequiredSources}\n";

                if ((x + 1) % pagehoist == 0 || (x + 1) == list.Count)
                {
                    pages.Add(pagetext);
                    pagetext = "";
                }
            }

            return pages;
        }

        public static IList<string> Paginate(this IReadOnlyList<IAmRole> roles, Guild sguild, IGuild guild, IGuildUser user, int pagehoist = 10)
        {
            if (sguild == null || guild == null) return null;

            var pages = new List<string>();

            var s = new StringBuilder();

            for (int x = 0; x < roles.Count; x++)
            {
                var rol = roles.ElementAt(x);
                var role = guild.GetRole(rol.RoleId);

                var sl = new StringBuilder($"{x + 1}. {role.Name} | ");

                if (rol.Price != 0)
                {
                    sl.Append($"Cost = {sguild.MoneyIcon}{rol.Price} | ");
                }
                if (rol.LevelRequired != 0)
                {
                    sl.Append($"Level = {rol.LevelRequired} | ");
                }
                if (rol.RequiredRoleId != 0)
                {
                    sl.Append($"Requires = {guild.GetRole(rol.RequiredRoleId).Name} | ");
                }

                if (user.RoleIds.Contains(rol.RoleId))
                {
                    s.Append($"~~{sl.ToString().Substring(0, sl.Length - 3)}~~ **|** Already Acquired");
                }
                else
                {
                    s.Append(sl.ToString().Substring(0, sl.Length - 3));
                }

                if ((x + 1) % pagehoist == 0 || (x + 1) == roles.Count)
                {
                    pages.Add(s.ToString());
                    s.Clear();
                }
                else
                {
                    s.AppendLine();
                }
            }

            return pages;
        }

        #endregion Pagination
    }
}