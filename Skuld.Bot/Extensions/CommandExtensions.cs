﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NodaTime;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Formatting;
using Skuld.Models;
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
			var config = Database.Configurations.Find(SkuldAppContext.ConfigurationId);

			var botusers = await guild.RobotMembersAsync().ConfigureAwait(false);
			var humanusers = await guild.HumanMembersAsync().ConfigureAwait(false);
			var ratio = await guild.GetBotUserRatioAsync().ConfigureAwait(false);
			var afkchan = await guild.GetAFKChannelAsync().ConfigureAwait(false);
			var owner = await guild.GetOwnerAsync().ConfigureAwait(false);
			var userratio = await guild.GetBotUserRatioAsync().ConfigureAwait(false);
			var channels = await guild.GetChannelsAsync().ConfigureAwait(false);
			var afktimeout = guild.AFKTimeout % 3600 / 60;

			var embed =
				new EmbedBuilder()
					.AddFooter(context)
					.WithTitle(guild.Name)
					.AddAuthor()
					.WithColor(EmbedExtensions.RandomEmbedColor())
					.AddInlineField("Users", $"Users: {humanusers.ToFormattedString()}\nBots: {botusers.ToFormattedString()}\nRatio: {ratio}%")
					.AddInlineField("Shard", client?.GetShardIdFor(guild))
					.AddInlineField("Verification Level", guild.VerificationLevel)
					.AddInlineField("Voice Region", guild.VoiceRegionId)
					.AddInlineField("Owner", $"{owner.Mention}")
					.AddInlineField("Text Channels", channels.Count(x => x.GetType() == typeof(SocketTextChannel)))
					.AddInlineField("Voice Channels", channels.Count(x => x.GetType() == typeof(SocketVoiceChannel)))
					.AddInlineField("AFK Timeout", afktimeout + " minutes")
					.AddInlineField("Default Notifications", guild.DefaultMessageNotifications)
					.AddInlineField("Created", guild.CreatedAt.ToDMYString() + "\t(DD/MM/YYYY)")
					.AddInlineField($"Emotes [{guild.Emotes.Count}]", $" Use `{skuldguild?.Prefix ?? config.Prefix}server-emojis` to view them")
					.AddInlineField($"Roles [{guild.Roles.Count}]", $" Use `{skuldguild?.Prefix ?? config.Prefix}server-roles` to view them");

			if (!string.IsNullOrEmpty(afkchan?.Name))
			{
				embed.AddInlineField("AFK Channel", $"[#{afkchan.Name}]({afkchan.JumpLink()})");
			}

			if (!string.IsNullOrEmpty(guild.IconUrl))
			{
				embed.WithThumbnailUrl(guild.IconUrl);
			}

			return embed.Build();
		}

		//https://stackoverflow.com/a/58497143
		public static bool IsDaylightSavingsTime(this ZonedDateTime timeInZone)
		{
			var instant = timeInZone.ToInstant();

			var zoneInterval = timeInZone.Zone.GetZoneInterval(instant);

			return zoneInterval.Savings != Offset.Zero;
		}

		#region Pagination

		public static IList<string> Paginate(this IReadOnlyList<MemeEndpoints> list, int pagehoist = 10)
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
			if (sguild is null || guild is null)
			{
				return null;
			}

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