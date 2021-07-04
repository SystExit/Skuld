﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NCalc;
using NodaTime;
using Skuld.Bot.Discord.Attributes;
using Skuld.Bot.Discord.Preconditions;
using Skuld.Bot.Extensions;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Formatting;
using Skuld.Core.Extensions.Verification;
using Skuld.Core.Helpers;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Services.Accounts.Banking.Models;
using Skuld.Services.Banking;
using Skuld.Services.Extensions;
using Skuld.Services.Globalization;
using Skuld.Services.Messaging.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Modules
{
	[
		Group,
		Name("Information"),
		RequireEnabledModule,
		Remarks("📚 Information Module")
	]
	public class InformationModule : ModuleBase<ShardedCommandContext>
	{
		public SkuldConfig Configuration { get; set; }
		public Locale Locale { get; set; }

		[
			Command("server"),
			Summary("Gets information about the server"),
			RequireContext(ContextType.Guild),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task GetServer()
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var dbguild = await
				Database.InsertOrGetGuildAsync(Context.Guild)
			.ConfigureAwait(false);

			Embed embed = await
				Context.Guild.GetSummaryAsync(Context.Client, Context, dbguild)
			.ConfigureAwait(false);

			await embed.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[
			Command("server-emojis"),
			RequireContext(ContextType.Guild),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task ServerEmoji()
		{
			var guild = Context.Guild;

			string message =
				$"Emojis of __**{guild.Name}**__ ({guild.Emotes.Count})\n";

			var num = 0;

			if (guild.Emotes.Count != 0)
			{
				foreach (var emoji in guild.Emotes)
				{
					num++;
					if (num % 5 != 0 || num == 0)
					{
						message +=
							$"{emoji.Name} <:{emoji.Name}:{emoji.Id}> | ";
					}
					else
					{
						message +=
							$"{emoji.Name} <:{emoji.Name}:{emoji.Id}>\n";
					}
				}
				message = message[0..^2];
			}

			await message.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[
			Command("server-roles"),
			RequireContext(ContextType.Guild),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task ServerRoles()
		{
			var guild = Context.Guild;
			var roles = guild.Roles.OrderByDescending(x => x.Position);
			string serverroles = null;

			foreach (var item in roles)
			{
				string thing = item.Name.TrimStart('@');

				serverroles += thing;

				if (item != roles.Last())
				{
					serverroles += ", ";
				}
			}

			string message =
				$"Roles of __**{guild.Name}**__ ({roles.Count()})\n";

			if (roles.Any())
			{
				message += "`" + serverroles + "`";
			}

			await message.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[
			Command("id-guild"),
			Summary("Get ID of Guild"),
			RequireContext(ContextType.Guild),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task GuildID()
		{
			await
				$"The ID of **{Context.Guild.Name}** is `{Context.Guild.Id}`"
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[
			Command("id"),
			Summary("Gets a users ID"),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task GetID([Remainder] IUser user = null)
		{
			if (user is null)
			{
				user = Context.User;
			}
			await
				$"The ID of **{user.FullName()}** is: `{user.Id}`"
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[
			Command("id"),
			Summary("Get id of channel"),
			RequireContext(ContextType.Guild),
			Ratelimit(20, 1, Measure.Minutes),
			Usage("#general")
		]
		public async Task ChanID(IChannel channel)
		{
			await
				$"The ID of **{channel?.Name}** is `{channel.Id}`"
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[
			Command("roleinfo"),
			RequireContext(ContextType.Guild),
			Ratelimit(20, 1, Measure.Minutes),
			Usage("Super Awesome Role")
		]
		public async Task RoleInfo([Remainder] IRole role)
		{
			var memberString = new StringBuilder();

			var orderedRoles = Context.Guild.Roles.OrderBy(x => x.Position);

			IRole previousRole = null;
			if (role != orderedRoles.FirstOrDefault())
			{
				previousRole =
					orderedRoles.ElementAtOrDefault(role.Position - 1);
			}
			IRole nextRole = null;
			if (role != orderedRoles.LastOrDefault())
			{
				nextRole = orderedRoles.ElementAtOrDefault(role.Position + 1);
			}

			var members = await
				Context.Guild.GetRoleMembersAsync(role)
			.ConfigureAwait(false);

			if (role != Context.Guild.EveryoneRole)
			{
				foreach (var member in members)
				{
					memberString.Append(member.Mention);

					if (member != members[members.Count - 1])
					{
						memberString.Append(", ");
					}
				}
			}
			else
			{
				memberString.Append(role.Mention);
				memberString.Append(' ');
				memberString.Append("😝");
			}

			string roleMembers = "";

			if (members.Count > 0)
			{
				roleMembers =
					memberString.Length <= 1024 ?
						memberString.ToString() :
						members.Count.ToFormattedString();
			}
			else
			{
				roleMembers = "Role has no members";
			}

			string RolePosition = $"{role.Position}";
			if (previousRole is not null)
			{
				RolePosition += "\nBelow " +
					$"{previousRole.Mention}({previousRole.Position})";
			}
			if (nextRole is not null)
			{
				RolePosition += "\nAbove " +
					$"{nextRole.Mention}({nextRole.Position})";
			}
			string MemberName = "Members [" +
				$"{members.Count.ToFormattedString()}" +
				"]";

			await
				new EmbedBuilder()
					.AddFooter(Context)
					.WithTitle(role.Name)
					.WithColor(role.Color)
					.AddAuthor()
					.AddInlineField("Hoisted", role.IsHoisted)
					.AddInlineField("Managed By Discord", role.IsManaged)
					.AddInlineField("Mentionable", role.IsMentionable)
					.AddInlineField("Position", RolePosition)
					.AddInlineField("Color", role.Color.ToHex())
					.AddField(MemberName, roleMembers)
					.AddField("Created", role.CreatedAt)
			.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[
			Command("screenshare"),
			Summary("Get's the screenshare channel link"),
			RequireContext(ContextType.Guild),
			RequireGuildVoiceChannel,
			Alias("sc"),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task Screenshare()
		{
			await
				(
					$"<https://discordapp.com/channels/" +
					$"{Context.Guild.Id}/" +
					$"{(Context.User as IGuildUser)?.VoiceChannel.Id}>"
				).QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[
			Command("support"),
			Summary("Gives discord invite"),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task DevDisc()
		{
			await "Join the support server! <https://discord.skuld.bot/discord?ref=bot>"
				.QueueMessageAsync(Context)
				.ConfigureAwait(false);
		}

		[
			Command("invite"),
			Summary("OAuth2 Invite"),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task BotInvite()
		{
			await
				$"Invite me! <https://discord.skuld.bot/bot?ref=bot>"
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[
			Command("userratio"),
			Summary("Gets the ratio of users to bots"),
			RequireContext(ContextType.Guild),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task HumanToBotRatio()
		{
			var bots = await Context.Guild.RobotMembersAsync()
				.ConfigureAwait(false);
			var users = await Context.Guild.HumanMembersAsync()
				.ConfigureAwait(false);
			var ratio = await Context.Guild.GetBotUserRatioAsync()
				.ConfigureAwait(false);

			await
				(
					$"Current Bots are: {bots}\n" +
					$"Current Users are: {users}\n" +
					$"Total Guild Users: {Context.Guild.MemberCount}\n" +
					$"{ratio}% of the Guild Users are bots"
				)
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[
			Command("avatar"),
			Summary("Gets your/target's avatar"),
			Ratelimit(20, 1, Measure.Minutes),
			Usage("<@0>")
		]
		public async Task Avatar([Remainder] IUser user = null)
		{
			if (user is null)
			{
				user = Context.User;
			}

			var avatar =
				user.GetAvatarUrl(ImageFormat.Auto, 512) ??
				user.GetDefaultAvatarUrl();

			Color color = Color.Teal;

			if (!Context.IsPrivate)
			{
				color = (user as IGuildUser)
					.GetHighestRoleColor(Context.Guild);
			}

			await
				EmbedExtensions.FromImage(
					avatar,
					$"Avatar for {user.Mention}\n[Link]({avatar})",
					Context
				)
				.WithTitle("Mirror")
				.WithColor(color)
			.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[
			Command("createinvite", RunMode = RunMode.Async),
			Summary("Creates a new invite to the guild"),
			RequireContext(ContextType.Guild),
			Ratelimit(20, 1, Measure.Minutes),
			Usage(
				"#general",
				"#general \"1d\"",
				"#general \"1d 30m\" 5",
				"#general \"1d 30m 5s\" 5 true",
				"#general \"1d 30m 5s\" 5 true true"
			)
		]
		public async Task NewInvite([NotNull] ITextChannel channel,
			string timespan = null,
			int? maxUses = null,
			bool permanent = true,
			bool unique = true)
		{
			var timeSpan = StringTimeHelper.Parse(timespan);

			IInviteMetadata invite =
				await channel.CreateInviteAsync(
					(int)timeSpan.Value.TotalSeconds,
					maxUses,
					!permanent,
					unique
			).ConfigureAwait(false);

			await ("I created the invite with the following settings:\n" +
				"```cs\n" +
				$"       Channel : {channel.Name}\n" +
				(!timespan.IsNullOrWhiteSpace() ? $"   Maximum Age : {timespan}\n" : "") +
				(maxUses.HasValue ? $"  Maximum Uses : {maxUses}\n" : "") +
				$"     Permanent : {!permanent}\n" +
				$"        Unique : {unique}" +
				$"```Here's the link: {invite.Url}")
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[
			Command("me"),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task Whois()
		{
			if (!Context.IsPrivate)
			{
				await
					GetProileAsync(Context.User as IGuildUser)
				.ConfigureAwait(false);
				return;
			}
			else
			{
				await
					(await Context.User.GetWhoisAsync(
						null,
						null,
						Configuration
					).ConfigureAwait(false)
				).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}
		}

		[
			Command("whois"),
			Summary("Get's information about a user"),
			Alias("user"),
			Ratelimit(20, 1, Measure.Minutes),
			Usage("<@0>")
		]
		public async Task GetProileAsync([Remainder] IUser whois = null)
		{
			if (!Context.IsPrivate)
			{
				if (whois is null)
				{
					whois = Context.User;
				}

				await
					(
						await whois.GetWhoisAsync(
							whois as IGuildUser,
							(whois as IGuildUser).RoleIds,
							Configuration)
						.ConfigureAwait(false)
					).QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
			else
			{
				await
					(
						await Context.User.GetWhoisAsync(
							null,
							null,
							Configuration
						).ConfigureAwait(false)
					).QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		[
			Command("roles"),
			Summary("Gets a users current roles"),
			RequireContext(ContextType.Guild),
			Ratelimit(20, 1, Measure.Minutes),
			Usage("<@0>")
		]
		public async Task GetRole(SocketGuildUser user = null)
		{
			if (user is null)
			{
				user = (SocketGuildUser)Context.User;
			}

			var guild = Context.Guild;
			var userroles = user.Roles;
			var roles =
				userroles.OrderByDescending(x => x.Position)
				.Select(x => x.Name)
				.Aggregate((current, next) => current + ", " + next)
				.Replace("@everyone", "everyone");
			await (
					$"Roles of __**{user.FullNameWithNickname()}**__ " +
					$"({userroles.Count})\n\n`{roles ?? "No roles"}`"
				)
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[
			Command("calc"),
			Summary("Calculates an expression"),
			Ratelimit(20, 1, Measure.Minutes),
			Usage("500^2 * 50")
		]
		public async Task Calculate([Remainder] string expression)
		{
			Expression exp = new(expression);

			exp.Parameters.Add("Pi", Math.PI);
			exp.Parameters.Add("e", Math.E);

			var res = exp.Evaluate();

			var result = res;

			if (result is string)
			{
				result = (result as string)
					.Replace("everyone", "\u00A0everyone");
			}

			await
				EmbedExtensions.FromMessage(
					"Calculator",
					$"Expression: `{expression}`\nResult: {result}",
					Context
				)
			.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[
			Command("youngest"),
			Summary("Get's the youngest created account in the server"),
			Ratelimit(20, 1, Measure.Minutes),
			RequireContext(ContextType.Guild)
		]
		public async Task GetYoungestMember()
		{
			var usrs = Context.Guild.Users.OrderByDescending(x => x.CreatedAt);

			var young = usrs.FirstOrDefault();

			await
				EmbedExtensions
					.FromMessage(
						"Server Members",
						"The youngest server member is " +
						$"{young.FullNameWithNickname()} - " +
						$"{young.Mention}",
						Context
					)
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[
			Command("oldest"),
			Summary("Get's the oldest created account in the server"),
			Ratelimit(20, 1, Measure.Minutes),
			RequireContext(ContextType.Guild)
		]
		public async Task GetOldestMember()
		{
			var usrs = Context.Guild.Users.OrderBy(x => x.CreatedAt);

			var old = usrs.FirstOrDefault();

			await
				EmbedExtensions
					.FromMessage(
						"Server Members",
						"The oldest server member is " +
						$"{old.FullNameWithNickname()} - " +
						$"{old.Mention}",
						Context
					)
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		#region Leaderboards

		[
			Name("Leaderboard"),
			Group("leaderboard"),
			Alias("lb"),
			Ratelimit(20, 1, Measure.Minutes),
			Remarks("🏆 Get's people's rankings")
		]
		public class LeaderboardModule : ModuleBase<ShardedCommandContext>
		{
			[
				Command("money"),
				Summary("Get the money leaderboard"),
				Usage("true", "false")
			]
			public async Task MoneyLeaderboard(bool global = true)
			{
				if (global)
				{
					await
						EmbedExtensions.FromInfo(
							"View the global money leaderboard at: " +
							$"{SkuldAppContext.LeaderboardMoney}",
							Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
				else
				{
					if (Context.IsPrivate)
					{
						await
							EmbedExtensions.FromError(
								"Local Money Leaderboard " +
								"is not supported in a DM",
								Context)
							.QueueMessageAsync(Context)
						.ConfigureAwait(false);
					}
					else
					{
						await EmbedExtensions.FromInfo(
							"View this server's money leaderboard at: " +
							$"{SkuldAppContext.LeaderboardMoney}/" +
							$"{Context.Guild.Id}",
							Context)
							.QueueMessageAsync(Context)
						.ConfigureAwait(false);
					}
				}
			}
			[
				Command("levels"),
				Summary("Get the experience leaderboard"),
				Usage("true", "false")
			]
			public async Task ExperienceLeaderboard(bool global = false)
			{
				if (global)
				{
					await EmbedExtensions.FromInfo(
						$"View the global experience leaderboard at: " +
						$"{SkuldAppContext.LeaderboardExperience}",
						Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
					return;
				}

				if (Context.IsPrivate)
				{
					await EmbedExtensions.FromError(
						"Local experience not supported in a DM",
						Context)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
					return;
				}

				GuildFeatures feats = null;
				Guild dbGuild;

				{
					using var database = new SkuldDbContextFactory()
						.CreateDbContext();

					dbGuild =
						await database
							.InsertOrGetGuildAsync(Context.Guild)
						.ConfigureAwait(false);

					feats = database.Features.Find(dbGuild.Id);
				}

				if (!feats.Experience && !global)
				{
					await
						EmbedExtensions.FromError(
							"Guild not opted into `Experience` module. " +
							$"Use: `{dbGuild.Prefix}guild feature levels 1`",
							Context
						).QueueMessageAsync(Context)
					.ConfigureAwait(false);
					return;
				}

				await
					EmbedExtensions.FromInfo(
						"View this server's experience leaderboard at: " +
						SkuldAppContext.LeaderboardExperience +
						$"/{Context.Guild.Id}",
						Context
					).QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			[
				Command("command"),
				Summary("Get the usage for the command specified"),
				Usage("ping")
			]
			public async Task GetCommandUsage([Remainder] string command)
			{
				bool existsButNoData = false;
				CommandLeaderboardInfo info = null;
				{
					using var Database = new SkuldDbContextFactory()
						.CreateDbContext();

					if (Database.CustomCommands.Any(x => x.Name == command))
					{
						var first =
							Database.CustomCommands
							.FirstOrDefault(x => x.Name == command);
						var usage =
							Database.UserCommandUsage
							.FirstOrDefault(x =>
							x.UserId == Context.User.Id && x.Command == command);
						var ranking = await
							Database.UserCommandUsage.AsAsyncEnumerable()
							.Where(x => x.Command == command)
							.OrderByDescending(x => x.Usage)
						.ToListAsync();

						if (first is not null && usage is not null && ranking.Any())
						{
							info = new CommandLeaderboardInfo
							{
								Name = first.Name,
								Usage = usage.Usage,
								Rank = (ulong)ranking.IndexOf(
									ranking.FirstOrDefault(x =>
									x.UserId == Context.User.Id)
								) + 1,
								Total = (ulong)ranking.Count
							};
						}
						else if (first is not null)
						{
							existsButNoData = true;
						}
					}
				}

				if (info is null)
				{
					var result = SkuldApp.CommandService.Search(command);

					if (result.IsSuccess)
					{
						using var Database = new SkuldDbContextFactory()
							.CreateDbContext();
						var usage = Database.UserCommandUsage
							.FirstOrDefault(x =>
							x.UserId == Context.User.Id && x.Command == command);
						var ranking = await
							Database.UserCommandUsage.AsAsyncEnumerable()
							.Where(x => x.Command == command)
							.OrderByDescending(x => x.Usage)
						.ToListAsync();

						if (usage is not null && ranking.Any())
						{
							info = new CommandLeaderboardInfo
							{
								Name = result.Commands[0]
									.Command.Name,
								Usage = usage.Usage,
								Rank = (ulong)ranking.IndexOf(
									ranking.FirstOrDefault(x =>
									x.UserId == Context.User.Id)
								) + 1,
								Total = (ulong)ranking.Count
							};
						}
						else
						{
							existsButNoData = true;
						}
					}
				}

				if (info is null && !existsButNoData)
				{
					await
						EmbedExtensions.FromError(
							$"Couldn't find a command like: `{command}`. " +
							"Please verify input and try again.",
							Context
						)
						.QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}
				else if (existsButNoData)
				{
					await
						EmbedExtensions.FromError(
							$"You haven't used the command: `{command}`. " +
							"Please use it and try again",
							Context
						)
						.QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}

				var rank = $"{info.Rank.ToFormattedString()}/" +
					info.Total.ToFormattedString();

				await
					new EmbedBuilder()
					.AddAuthor()
					.AddFooter(Context)
					.AddInlineField("Command", info.Name)
					.AddInlineField("Usage", info.Usage.ToFormattedString())
					.AddInlineField("Rank", rank)
				.QueueMessageAsync(Context).ConfigureAwait(false);
			}

			private class CommandLeaderboardInfo
			{
				public string Name;
				public ulong Usage;
				public ulong Rank;
				public ulong Total;
			}
		}

		#endregion Leaderboards

		#region Time

		[
			Command("time"),
			Summary("Gets current time"),
			Ratelimit(20, 1, Measure.Minutes)
		]
		public async Task Time()
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var sUser = await
				Database.InsertOrGetUserAsync(Context.User)
			.ConfigureAwait(false);

			var sGuild = await
				Database.InsertOrGetGuildAsync(Context.Guild)
			.ConfigureAwait(false);

			ZonedDateTime timeZone = Instant.FromDateTimeUtc(DateTime.UtcNow)
				.InZone(
					DateTimeZoneProviders.Tzdb.GetZoneOrNull(sUser.TimeZone)
			);

			var time = timeZone.ToDateTimeUnspecified().ToDMYString();

			if (sUser.TimeZone is not null)
			{
				await
					$"Your time is currently; {time}"
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
			else
			{
				var prefix = sGuild.Prefix ?? Configuration.Prefix;

				var message =
					$"UTC Time is: {DateTime.UtcNow.ToDMYString()}\n" +
					$"Please give me your time zone using {prefix}settimezone";

				await message.QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[
			Command("time"),
			Summary("Converts a time to a set of times"),
			Ratelimit(20, 1, Measure.Minutes),
			Usage("<@0> @Friend2")
		]
		public async Task ConvertTime(params IGuildUser[] users)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			StringBuilder message = new("```cs\n");

			foreach (var user in users)
			{
				try
				{
					if (user.IsBot ||
						user.IsWebhook ||
						user.Discriminator == "0000" ||
						user.DiscriminatorValue == 0)
					{
						continue;
					}

					var sUser = await
						Database.InsertOrGetUserAsync(user)
					.ConfigureAwait(false);

					if (sUser.TimeZone is not null)
					{
						ZonedDateTime timeZone =
							Instant.FromDateTimeUtc(DateTime.UtcNow)
							.InZone(
								DateTimeZoneProviders.Tzdb.GetZoneOrNull(
									sUser.TimeZone
								)
							);

						var time = timeZone
							.ToDateTimeUnspecified()
						.ToDMYString();

						message.Append("\"The time for ");
						message.Append(user.Username);
						message.Append(" is: ");
						message.Append($"{time}\"\n");
					}
					else
					{
						message.Append('"');
						message.Append(sUser.Username);
						message.Append(" has not given me their timezone\"\n");
					}
				}
				catch
				{
				}
			}

			message.Append("```");

			await message.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		#endregion Time

		#region IAmRole

		[
			Command("addrole"),
			Summary("Adds yourself to a role"),
			Alias("iam"),
			RequireDatabase,
			Ratelimit(20, 1, Measure.Minutes),
			Usage("5")
		]
		public async Task IamRole(int page = 0)
		{
			if (page != 0)
				page -= 1;

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			if (Context.IsPrivate)
			{
				await
					EmbedExtensions.FromError(
						"DM's are not supported for this command",
						Context
					).QueueMessageAsync(Context)
				.ConfigureAwait(false);
				return;
			}

			var iamlist = await Database.IAmRoles
				.ToAsyncEnumerable()
				.Where(x => x.GuildId == Context.Guild.Id)
			.ToListAsync();

			if (iamlist.Any())
			{
				var gld =
					await Database
						.InsertOrGetGuildAsync(Context.Guild)
					.ConfigureAwait(false);

				var paged = iamlist.Paginate(
					gld,
					Context.Guild,
					Context.Guild.GetUser(Context.User.Id)
				);

				if (page >= paged.Count)
				{
					await
						EmbedExtensions.FromError(
							$"There are only {paged.Count} " +
							$"pages to scroll through",
							Context
					).QueueMessageAsync(Context)
					.ConfigureAwait(false);
					return;
				}

				await
					EmbedExtensions.FromMessage(
						$"Joinable roles of " +
						$"__{Context.Guild.Name}__ {page + 1}/{paged.Count}",
						paged[page],
						Context
					).QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
			else
			{
				await
					EmbedExtensions.FromError(
						$"{Context.Guild.Name} has no joinable roles",
						Context
					).QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		[Command("addrole"), Summary("Adds yourself to a role")]
		[Alias("iam"), RequireDatabase]
		[Ratelimit(20, 1, Measure.Minutes)]
		[Usage("Amazing Role")]
		public async Task IamRole([Remainder] IRole role)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			if (Context.IsPrivate)
			{
				await
					EmbedExtensions.FromError(
						"DM's are not supported for this command",
						Context
					).QueueMessageAsync(Context)
				.ConfigureAwait(false);
				return;
			}

			var iamlist = await Database.IAmRoles
				.ToAsyncEnumerable()
				.Where(x => x.GuildId == Context.Guild.Id)
			.ToListAsync();

			if (iamlist.Any())
			{
				var r = iamlist.FirstOrDefault(x => x.RoleId == role.Id);

				if (r is not null)
				{
					var gUsr = Context.User as IGuildUser;
					var usr = await Database
						.InsertOrGetUserAsync(Context.User)
					.ConfigureAwait(false);
					var gld = await Database
						.InsertOrGetGuildAsync(Context.Guild)
					.ConfigureAwait(false);

					var didpass = CheckIAmValidAsync(usr,
						gUsr,
						gld,
						Context.Guild,
						r
					);

					if (didpass != IAmFail.Success)
					{
						await
							EmbedExtensions.FromError(
								GetErrorIAmFail(
									didpass,
									r,
									gld,
									Context.Guild
								),
								Context)
							.QueueMessageAsync(Context)
						.ConfigureAwait(false);
					}
					else
					{
						if (gUsr.RoleIds.Contains(r.RoleId))
						{
							await
								EmbedExtensions.FromError(
									"You already have that role",
									Context
								).QueueMessageAsync(Context)
							.ConfigureAwait(false);
							return;
						}

						try
						{
							var ro = Context.Guild.GetRole(r.RoleId);

							await
								gUsr.AddRoleAsync(ro)
							.ConfigureAwait(false);

							await
								EmbedExtensions.FromSuccess(
									$"You now have the role \"{ro.Name}\"",
									Context
								).QueueMessageAsync(Context)
							.ConfigureAwait(false);

							if (r.Price > 0)
							{
								TransactionService.DoTransaction(
									new TransactionStruct
									{
										Amount = r.Price,
										Sender = usr
									}
								);

								await
									Database.SaveChangesAsync()
								.ConfigureAwait(false);
							}
						}
						catch (Exception ex)
						{
							if (ex.Message.Contains("403"))
							{
								await
									EmbedExtensions.FromError(
										"I need to be above the role and " +
										"have the permission `MANAGE_ROLES` " +
										"in order to give the role",
										Context
									).QueueMessageAsync(Context)
								.ConfigureAwait(false);
							}
							else
							{
								await
									EmbedExtensions.FromError(
										ex.Message,
										Context
									).QueueMessageAsync(Context)
								.ConfigureAwait(false);
							}
							Log.Error(
								SkuldAppContext.GetCaller(),
								ex.Message,
								Context,
								ex
							);
						}
					}
				}
				else
				{
					await
						EmbedExtensions.FromError(
							"Role isn\'t configured to be self assignable",
							Context
						).QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
			}
			else
			{
				await
					EmbedExtensions.FromInfo(
						$"{Context.Guild.Name} has no joinable roles",
						Context
					).QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		[Command("removerole"), Summary("Removes yourself from a role")]
		[Alias("iamnot"), RequireDatabase]
		[Ratelimit(20, 1, Measure.Minutes)]
		[Usage("Amazing Role")]
		public async Task IamNotRole([Remainder] IRole role)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var g = Context.User as IGuildUser;

			var iamlist = await
				Database.IAmRoles.ToAsyncEnumerable()
				.Where(x => x.GuildId == Context.Guild.Id)
			.ToListAsync();

			if (iamlist.Any(x => x.RoleId == role.Id))
			{
				if (g.RoleIds.Contains(role.Id))
				{
					try
					{
						await g.RemoveRoleAsync(role).ConfigureAwait(false);
						await
							EmbedExtensions.FromSuccess(
								$"You are no longer \"{role.Name}\"",
								Context
							).QueueMessageAsync(Context)
						.ConfigureAwait(false);
					}
					catch (Exception ex)
					{
						if (ex.Message.Contains("403"))
						{
							await
								EmbedExtensions.FromError(
									"Ensure that I have `MANAGE_ROLES` " +
									"and that " +
									$"I am above the role \"{role.Name}\" " +
									"in order to remove it",
									Context
								).QueueMessageAsync(Context)
							.ConfigureAwait(false);
						}
						else
						{
							await
								EmbedExtensions.FromError(
									ex.Message,
									Context
								).QueueMessageAsync(Context)
							.ConfigureAwait(false);
						}
						Log.Error(
							SkuldAppContext.GetCaller(),
							ex.Message,
							Context,
							ex
						);
					}
				}
				else
				{
					await
						EmbedExtensions.FromInfo(
							"You don\'t have that role",
							Context
						).QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
			}
			else
			{
				await
					EmbedExtensions.FromError(
						"Role isn\'t configured to be self assignable",
						Context
					).QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		private string GetErrorIAmFail(
			IAmFail amFail,
			IAmRole role,
			Guild sguild,
			IGuild guild
		)
		{
			var Prefix = "You don\'t have";
			return amFail switch
			{
				IAmFail.Price => $"{Prefix} enough money. " +
						$"You need at least {sguild.MoneyIcon}{role.Price}",
				IAmFail.Level =>
						$"{Prefix} the level required for this role " +
						$"(Level: {role.LevelRequired})",
				IAmFail.RequiredRole =>
						$"{Prefix} the required role for this role. " +
						"You need the role " +
						$"\"{guild.GetRole(role.RequiredRoleId).Name}\"",
				_ => "",
			};
		}

		private IAmFail CheckIAmValidAsync(
			User suser,
			IGuildUser user,
			Guild sguild,
			IGuild guild,
			IAmRole roleconf
		)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			if (roleconf.RequiredRoleId != 0)
			{
				if (!user.RoleIds.Any(x => x == roleconf.RequiredRoleId))
				{
					return IAmFail.RequiredRole;
				}
			}

			if (suser.Money < roleconf.Price)
			{
				return IAmFail.Price;
			}

			var guildExperience = Database.UserXp
				.FirstOrDefault(
					x =>
						x.UserId == suser.Id &&
						x.GuildId == guild.Id
				);

			if (guildExperience is not null)
			{
				if (guildExperience.Level < roleconf.LevelRequired &&
					Database.Features
					.Find(sguild.Id)
					.Experience
				)
				{
					return IAmFail.Level;
				}
			}
			else if (roleconf.LevelRequired != 0)
			{
				return IAmFail.Level;
			}

			return IAmFail.Success;
		}

		#endregion IAmRole
	}
}