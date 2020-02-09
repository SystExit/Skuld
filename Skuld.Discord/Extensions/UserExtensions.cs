using Discord;
using Discord.WebSocket;
using NodaTime;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Formatting;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Discord.Extensions
{
    public static class UserExtensions
    {
        public static string FullName(this IUser usr)
            => $"{usr.Username}#{usr.Discriminator}";

        public static string FullNameWithNickname(this IGuildUser usr)
        {
            if (usr.Nickname == null)
                return usr.FullName();
            else
                return $"{usr.Username} ({usr.Nickname})#{usr.Discriminator}";
        }

        public static async Task<object> GrantExperienceAsync(this User user, ulong amount, IGuild guild, bool skipTimeCheck = false)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var luxp = Database.UserXp.FirstOrDefault(x => x.UserId == user.Id && x.GuildId == guild.Id);

            bool didLevelUp = false;

            if (luxp != null)
            {
                ulong levelAmount = 0;

                var xptonextlevel = DatabaseUtilities.GetXPLevelRequirement(luxp.Level + 1, DiscordUtilities.PHI); //get next level xp requirement based on phi
                while ((luxp.XP + amount) >= xptonextlevel)
                {
                    xptonextlevel = DatabaseUtilities.GetXPLevelRequirement(luxp.Level + 1 + levelAmount, DiscordUtilities.PHI);
                    levelAmount++;
                }

                if (luxp.LastGranted < (DateTime.UtcNow.ToEpoch() - 60) || skipTimeCheck)
                {
                    DogStatsd.Increment("user.levels.processed");
                    if (levelAmount > 0) //if over or equal to next level requirement, update accordingly
                    {
                        luxp.XP = 0;
                        luxp.TotalXP += amount;
                        luxp.Level += levelAmount;
                        luxp.LastGranted = DateTime.UtcNow.ToEpoch();

                        didLevelUp = true;
                    }
                    else
                    {
                        luxp.XP += amount;
                        luxp.TotalXP += amount;
                        luxp.LastGranted = DateTime.UtcNow.ToEpoch();

                        didLevelUp = false;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Database.UserXp.Add(new UserExperience
                {
                    LastGranted = DateTime.UtcNow.ToEpoch(),
                    XP = amount,
                    UserId = user.Id,
                    GuildId = guild.Id,
                    TotalXP = amount,
                    Level = DatabaseUtilities.GetLevelFromTotalXP(amount, DiscordUtilities.PHI)
                });

                didLevelUp = false;

                DogStatsd.Increment("user.levels.processed");
            }

            await Database.SaveChangesAsync().ConfigureAwait(false);

            return didLevelUp;
        }

        public static async Task<EmbedBuilder> GetWhoisAsync(this IUser user, IGuildUser guildUser, IReadOnlyCollection<ulong> roles, IDiscordClient Client, SkuldConfig Configuration)
        {
            using var Database = new SkuldDbContextFactory().CreateDbContext();
            var sUser = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

            string status;
            if (user.Activity != null)
            {
                if (user.Activity.Type == ActivityType.Streaming) status = DiscordUtilities.Streaming_Emote.ToString();
                else status = user.Status.StatusToEmote();
            }
            else status = user.Status.StatusToEmote();

            var embed = new EmbedBuilder()
                .AddAuthor(Client)
                .WithTitle($"{user.Username}{(guildUser != null ? (guildUser.Nickname != null ? $" ({guildUser.Nickname})" : "") : "")}#{user.DiscriminatorValue}")
                .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithColor(guildUser?.GetHighestRoleColor(guildUser?.Guild) ?? EmbedExtensions.RandomEmbedColor());

            embed.AddInlineField(":id: User ID", user.Id.ToString() ?? "Unknown");
            embed.AddInlineField(":vertical_traffic_light: Status", status ?? "Unknown");

            if (user.Activity != null)
            {
                embed.AddInlineField(":video_game: Status", user.Activity.ActivityToString());
            }

            embed.AddInlineField("🤖 Is a bot?", user.IsBot ? "✔️" : "❌");

            embed.AddInlineField("👀 Mutual Servers", $"{(user as SocketUser).MutualGuilds.Count}");

            StringBuilder clientString = new StringBuilder();
            foreach (var client in user.ActiveClients)
            {
                clientString = clientString.Append(client.ToEmoji());

                if (user.ActiveClients.Count > 1 && client != user.ActiveClients.LastOrDefault())
                    clientString.Append(", ");
            }

            if(user.ActiveClients.Any())
            {
                embed.AddInlineField($"Active Client{(user.ActiveClients.Count > 1 ? "s" : "")}", $"{clientString}");
            }

            if (roles != null)
            {
                embed.AddField(":shield: Roles", $"[{roles.Count}] Do `{Configuration.Prefix}roles` to see your roles");
            }

            if (sUser.TimeZone != null)
            {
                var time = Instant.FromDateTimeUtc(DateTime.UtcNow).InZone(DateTimeZoneProviders.Tzdb.GetZoneOrNull(sUser.TimeZone)).ToDateTimeUnspecified().ToDMYString();

                embed.AddField("Current Time", $"{time}\t`DD/MM/YYYY HH:mm:ss`");
            }

            var createdatstring = user.CreatedAt.GetStringfromOffset(DateTime.UtcNow);
            embed.AddField(":globe_with_meridians: Discord Join", user.CreatedAt.ToDMYString() + $" ({createdatstring})\t`DD/MM/YYYY`");

            if (guildUser != null)
            {
                var joinedatstring = guildUser.JoinedAt.Value.GetStringfromOffset(DateTime.UtcNow);
                embed.AddField(":inbox_tray: Server Join", guildUser.JoinedAt.Value.ToDMYString() + $" ({joinedatstring})\t`DD/MM/YYYY`");
            }

            if (guildUser.PremiumSince.HasValue)
            {
                var icon = guildUser.PremiumSince.Value.BoostMonthToEmote();

                var offsetString = guildUser.PremiumSince.Value.GetStringfromOffset(DateTime.UtcNow);

                embed.AddField(DiscordUtilities.NitroBoostEmote + " Boosting Since", $"{(icon == null ? "" : icon + " ")}{guildUser.PremiumSince.Value.UtcDateTime.ToDMYString()} ({offsetString})\t`DD/MM/YYYY`");
            }

            return embed;
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
            UserStatus.Online => DiscordUtilities.Online_Emote.ToString(),
            UserStatus.AFK => DiscordUtilities.Idle_Emote.ToString(),
            UserStatus.Idle => DiscordUtilities.Idle_Emote.ToString(),
            UserStatus.DoNotDisturb => DiscordUtilities.DoNotDisturb_Emote.ToString(),
            UserStatus.Invisible => DiscordUtilities.Offline_Emote.ToString(),
            UserStatus.Offline => DiscordUtilities.Offline_Emote.ToString(),

            _ => DiscordUtilities.Offline_Emote.ToString(),
        };

        public static string BoostMonthToEmote(this DateTimeOffset boostMonths)
        {
            var months = DateTime.UtcNow.MonthsBetween(boostMonths.Date);

            if (months <= 1)
            {
                return DiscordUtilities.NitroBoostRank1Emote.ToString();
            }
            if (months == 2)
            {
                return DiscordUtilities.NitroBoostRank2Emote.ToString();
            }
            if (months == 3)
            {
                return DiscordUtilities.NitroBoostRank3Emote.ToString();
            }
            if (months >= 4)
            {
                return DiscordUtilities.NitroBoostRank4Emote.ToString();
            }

            return null;
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

        public static async Task<int> RobotMembersAsync(this IGuild guild)
        {
            await guild.DownloadUsersAsync().ConfigureAwait(false);

            return (await guild.GetUsersAsync(CacheMode.AllowDownload)).Count(x => x.IsBot);
        }

        public static async Task<int> HumanMembersAsync(this IGuild guild)
        {
            await guild.DownloadUsersAsync().ConfigureAwait(false);

            return (await guild.GetUsersAsync(CacheMode.AllowDownload)).Count(x => !x.IsBot);
        }

        public static async Task<decimal> GetBotUserRatioAsync(this IGuild guild)
        {
            await guild.DownloadUsersAsync().ConfigureAwait(false);
            var botusers = await guild.RobotMembersAsync().ConfigureAwait(false);
            var userslist = await guild.GetUsersAsync().ConfigureAwait(false);
            var users = userslist.Count;
            return Math.Round((((decimal)botusers / users) * 100m), 2);
        }
    }
}