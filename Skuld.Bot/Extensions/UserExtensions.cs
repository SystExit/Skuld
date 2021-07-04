using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NodaTime;
using Skuld.Bot.Extensions;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Formatting;
using Skuld.Core.Models;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Services.Accounts.Banking.Models;
using Skuld.Services.Banking;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skuld.Bot.Extensions
{
	public static class UserExtensions
	{
		public static async Task<EmbedBuilder> GetWhoisAsync(
			this IUser user,
			IGuildUser guildUser,
			IReadOnlyCollection<ulong> roles,
			SkuldConfig Configuration)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();
			var sUser = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

			string status = user.Status.StatusToEmote();
			if (user.Activities is not null && user.Activities.Count > 0)
			{
				if (user.Activities.Any(a => a.Type == ActivityType.Streaming)) status = DiscordUtilities.Streaming_Emote.ToString();
			}

			var embed = new EmbedBuilder()
				.AddAuthor()
				.WithTitle(guildUser is not null ? guildUser.FullNameWithNickname() : user.FullName())
				.WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
				.WithColor(guildUser?.GetHighestRoleColor(guildUser?.Guild) ?? EmbedExtensions.RandomEmbedColor());

			embed.AddInlineField(":id: User ID", Convert.ToString(user.Id, CultureInfo.InvariantCulture) ?? "Unknown");
			embed.AddInlineField(":vertical_traffic_light: Status", status ?? "Unknown");

			if (user.Activities is not null && user.Activities.Count > 0)
			{
				string ActivityTypeToEmoji(ActivityType type)
					=> type switch
					{
						ActivityType.Playing => "🎮",
						ActivityType.Watching => "📺",
						ActivityType.Listening => "🎧",
						ActivityType.Streaming => "📡",
						_ => "📜"
					};

				string activities = string.Join("\n", user.Activities.Select(a => $"{ActivityTypeToEmoji(a.Type)} {a.Name}{(!string.IsNullOrEmpty(a.Details) ? $" - {a.Details}" : "")}"));
			}
				/*if (user.Activity is not null)
				{
					embed.AddInlineField(":video_game: Status", user.Activity.ActivityToString());
				}*/

				embed.AddInlineField("🤖 Bot", user.IsBot ? DiscordUtilities.Tick_Emote : DiscordUtilities.Cross_Emote);

			embed.AddInlineField("👀 Mutual Servers", $"{(user as SocketUser).MutualGuilds.Count}");

			StringBuilder clientString = new();
			foreach (var client in user.ActiveClients)
			{
				clientString = clientString.Append(client.ToEmoji());

				if (user.ActiveClients.Count > 1 && client != user.ActiveClients.LastOrDefault())
					clientString.Append(", ");
			}

			if (user.ActiveClients.Any())
			{
				embed.AddInlineField($"Active Client{(user.ActiveClients.Count > 1 ? "s" : "")}", $"{clientString}");
			}

			if (roles is not null)
			{
				embed.AddField(":shield: Roles", $"[{roles.Count}] Do `{Configuration.Prefix}roles` to see your roles");
			}

			if (sUser.TimeZone is not null)
			{
				var time = Instant.FromDateTimeUtc(DateTime.UtcNow).InZone(DateTimeZoneProviders.Tzdb.GetZoneOrNull(sUser.TimeZone)).ToDateTimeUnspecified().ToDMYString();

				embed.AddField("Current Time", $"{time}\t`DD/MM/YYYY HH:mm:ss`");
			}

			var createdatstring = user.CreatedAt.GetStringFromOffset(DateTime.UtcNow);
			embed.AddField(":globe_with_meridians: Discord Join", user.CreatedAt.ToDMYString() + $" ({createdatstring})\t`DD/MM/YYYY`");

			if (guildUser is not null)
			{
				var joinedatstring = guildUser.JoinedAt.Value.GetStringFromOffset(DateTime.UtcNow);
				embed.AddField(":inbox_tray: Server Join", guildUser.JoinedAt.Value.ToDMYString() + $" ({joinedatstring})\t`DD/MM/YYYY`");
			}

			if (guildUser.PremiumSince.HasValue)
			{
				var icon = guildUser.PremiumSince.Value.BoostMonthToEmote();

				var offsetString = guildUser.PremiumSince.Value.GetStringFromOffset(DateTime.UtcNow);

				embed.AddField(DiscordUtilities.NitroBoostEmote + " Boosting Since", $"{(icon is null ? "" : icon + " ")}{guildUser.PremiumSince.Value.UtcDateTime.ToDMYString()} ({offsetString})\t`DD/MM/YYYY`");
			}

			return embed;
		}

		public static bool IsStreakReset(
			this User target,
			SkuldConfig config)
		{
			var days = target.IsDonator ? config.StreakLimitDays * 2 : config.StreakLimitDays;

			var limit = target.LastDaily.FromEpoch().Date.AddDays(days).ToEpoch();

			return DateTime.UtcNow.Date.ToEpoch() > limit;
		}

		public static ulong GetDailyAmount(
			[NotNull] this User target,
			[NotNull] SkuldConfig config
		)
		{
			var daily = config.DailyAmount;

			var mod = target.IsDonator ? 2U : 1;

			if (target.Streak > 0)
			{
				var amount = daily * Math.Min(50, target.Streak);

				return amount * mod;
			}

			return daily * mod;
		}

		public static bool ProcessDaily(
			this User target,
			ulong amount,
			User donor = null
		)
		{
			bool wasSuccessful = false;

			if (donor is null)
			{
				donor = target;
			}

			if (donor.LastDaily is 0 || donor.LastDaily < DateTime.UtcNow.Date.ToEpoch())
			{
				TransactionService.DoTransaction(new TransactionStruct
				{
					Amount = amount,
					Receiver = target
				});

				donor.LastDaily = DateTime.UtcNow.ToEpoch();

				wasSuccessful = true;
			}

			return wasSuccessful;
		}
	}
}