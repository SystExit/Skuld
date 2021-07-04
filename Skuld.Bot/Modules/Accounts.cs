using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Skuld.API;
using Skuld.Bot.Discord.Attributes;
using Skuld.Bot.Discord.Preconditions;
using Skuld.Bot.Extensions;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Conversion;
using Skuld.Core.Extensions.Formatting;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Services.Accounts.Banking.Models;
using Skuld.Services.Banking;
using Skuld.Services.Extensions;
using Skuld.Services.Messaging.Extensions;
using StatsdClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Skuld.Bot.Modules
{
	[Group, Name("Accounts"), RequireEnabledModule, RequireDatabase]
	[Remarks("👤 Skuld Accounts")]
	public class AccountModule : InteractiveBase<ShardedCommandContext>
	{
		public SkuldConfig Configuration { get; set; }
		public ISkuldAPIClient ApiClient { get; set; }
		const int PrestigeRequirement = 100;

		[Command("prestige"), Summary("Prestiges")]
		public async Task Prestige()
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

			var nextPrestige = usr.PrestigeLevel + 1;

			var xp = Database.UserXp.ToList().Where(x => x.UserId == usr.Id);

			ulong globalLevel = 0;

			xp.ToList().ForEach(x =>
			{
				globalLevel += x.Level;
			});

			if (globalLevel > PrestigeRequirement * nextPrestige)
			{
				await
					EmbedExtensions.FromInfo("Prestige Corner", "Please respond with y/n to confirm you want to prestige\n\n**__PLEASE NOTE__**\nThis will remove **ALL** experience gotten in every server with experience enabled", Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);

				var next = await NextMessageAsync().ConfigureAwait(false);

				try
				{
					if (next.Content.ToBool())
					{
						Database.UserXp.RemoveRange(Database.UserXp.ToList().Where(x => x.UserId == Context.User.Id));
						await Database.SaveChangesAsync().ConfigureAwait(false);

						usr.PrestigeLevel = usr.PrestigeLevel.Add(1);

						await Database.SaveChangesAsync().ConfigureAwait(false);
					}
				}
				catch
				{
					DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:parse-fail" });
				}
			}
			else
			{
				await
					EmbedExtensions.FromInfo("Prestige Corner", $"You lack the amount of levels required to prestige. For your prestige level ({usr.PrestigeLevel}), it is: {PrestigeRequirement * nextPrestige}", Context)
					.QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}
		}

		[Command("money"), Summary("Gets a users money")]
		[Alias("balance", "credits")]
		public async Task Money([Remainder] IGuildUser user = null)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			if (user is not null && (user.IsBot || user.IsWebhook))
			{
				await EmbedExtensions.FromError("SkuldBank - Account Information", DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			if (user is null)
				user = (IGuildUser)Context.User;

			var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);
			var dbusr = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

			await
				EmbedExtensions.FromMessage("SkuldBank - Account Information", $"{user.Mention} has {gld.MoneyIcon}{dbusr.Money.ToFormattedString()} {gld.MoneyName}", Context)
			.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("profile"), Summary("Get a users profile")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Profile([Remainder] IGuildUser user = null)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			if (user is not null && (user.IsBot || user.IsWebhook))
			{
				await EmbedExtensions.FromError(DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			if (user is null)
			{
				user = Context.User as IGuildUser;
			}

			await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

			var profileImage = await ApiClient.GetProfileCardAsync(Context.User.Id, Context.Guild.Id);

			if (profileImage is not null)
			{
				await "".QueueMessageAsync(Context, filestream: profileImage, filename: "image.png").ConfigureAwait(false);
			}
		}

		[Command("daily"), Summary("Daily Money")]
		public async Task Daily(IGuildUser user = null)
		{
			if (user is not null && (user.IsBot || user.IsWebhook))
			{
				await EmbedExtensions.FromError(DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var self = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

			var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

			User target = null;

			if (user is not null)
			{
				target = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);
			}

			var previousAmount = user is null ? self.Money : target.Money;

			if (self.IsStreakReset(Configuration))
			{
				self.Streak = 0;
			}

			ulong MoneyAmount = self.GetDailyAmount(Configuration);

			if ((user is null ? self : target).ProcessDaily(MoneyAmount, self))
			{
				string desc = $"You just got your daily of {gld.MoneyIcon}{MoneyAmount}";

				if (target is not null)
				{
					desc = $"You just gave your daily of {gld.MoneyIcon}{MoneyAmount.ToFormattedString()} to {user.Mention}";
				}

				var newAmount = user is null ? self.Money : target.Money;

				var embed =
					EmbedExtensions
					.FromMessage("SkuldBank - Daily",
						desc,
						Context
					)
					.AddInlineField(
						"Previous Amount",
						$"{gld.MoneyIcon}{previousAmount.ToFormattedString()}"
					)
					.AddInlineField(
						"New Amount",
						$"{gld.MoneyIcon}{newAmount.ToFormattedString()}"
					);

				if (self.Streak > 0)
				{
					embed.AddField(
						"Streak",
						$"You're on a streak of {self.Streak} days!!"
					);
				}

				self.Streak = self.Streak.Add(1);

				if (self.MaxStreak < self.Streak)
				{
					self.MaxStreak = self.Streak;
				}

				await embed.QueueMessageAsync(Context).ConfigureAwait(false);
			}
			else
			{
				TimeSpan remain = DateTime.UtcNow.Date.AddDays(1) - DateTime.UtcNow;

				await
					EmbedExtensions.FromError(
						"SkuldBank - Daily",
						$"You must wait `{remain.ToDifferenceString()}`",
						Context
					).QueueMessageAsync(Context)
				.ConfigureAwait(false);
			}

			await Database.SaveChangesAsync().ConfigureAwait(false);
		}

		[Command("give"), Summary("Give your money to people")]
		public async Task Give(IGuildUser user, ulong amount)
		{
			if (user == Context.User)
			{
				await EmbedExtensions.FromError("SkuldBank - Transaction", "Can't give yourself money", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}
			if (user is not null && (user.IsBot || user.IsWebhook))
			{
				await EmbedExtensions.FromError("SkuldBank - Transaction", DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

			if (usr.Money >= amount)
			{
				var usr2 = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

				TransactionService.DoTransaction(new TransactionStruct
				{
					Amount = amount,
					Sender = usr,
					Receiver = usr2
				});

				await Database.SaveChangesAsync().ConfigureAwait(false);

				var dbGuild = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

				await
					EmbedExtensions.FromMessage("Skuld Bank - Transaction", $"{Context.User.Mention} just gave {user.Mention} {dbGuild.MoneyIcon}{amount.ToFormattedString()}", Context)
					.QueueMessageAsync(Context).ConfigureAwait(false);

				DogStatsd.Increment("bank.transactions");
				DogStatsd.Increment("bank.money.transferred", (int)amount);
			}
			else
			{
				await EmbedExtensions.FromError($"{Context.User.Mention} you can't give more than you have", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[Command("rank"), Summary("Gets yours or someone's current text level")]
		[Alias("exp")]
		[Ratelimit(20, 1, Measure.Minutes)]
		public async Task Level(IGuildUser user = null)
		{
			if (Context.IsPrivate)
			{
				await EmbedExtensions.FromError("Cannot be run in private channel", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			if (user is not null && (user.IsBot || user.IsWebhook))
			{
				await EmbedExtensions.FromError(DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			if (user is null)
			{
				user = (IGuildUser)Context.User;
			}

			var image = await ApiClient.GetRankCardAsync(user.Id, Context.Guild.Id);

			if (image is not null)
			{
				await "".QueueMessageAsync(Context, filestream: image, filename: "image.png").ConfigureAwait(false);
			}
		}

		[Command("rep"), Summary("Gives someone rep or checks your rep")]
		[Ratelimit(20, 1, Measure.Minutes)]
		[Usage("<@0>")]
		public async Task GiveRep([Remainder] IGuildUser user = null)
		{
			if (user is not null && (user.IsBot || user.IsWebhook))
			{
				await EmbedExtensions.FromError(DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			if ((user is not null && user.Id == Context.User.Id) || user is null)
			{
				var count = Database.Reputations.Count(x => x.Repee == Context.User.Id);

				if (count > 0)
				{
					var ordered = (await Database.Reputations.ToListAsync()).OrderByDescending(x => x.Timestamp);
					var mostRecent = ordered.FirstOrDefault(x => x.Repee == Context.User.Id);

					await $"Your repuation is at: {count}rep\nYour most recent rep was by {Context.Client.GetUser(mostRecent.Reper).FullName()} at {mostRecent.Timestamp.FromEpoch()}"
						.QueueMessageAsync(Context).ConfigureAwait(false);
				}
				else
				{
					await "You have no reputation".QueueMessageAsync(Context).ConfigureAwait(false);
				}
				return;
			}

			var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);
			var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);
			var repee = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

			if (Database.Reputations.Any(x => x.Repee == repee.Id && x.Reper == Context.User.Id))
			{
				await "You have already given this person a reputation point.".QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			Database.Reputations.Add(new Reputation
			{
				Repee = repee.Id,
				Reper = usr.Id,
				Timestamp = DateTime.UtcNow.ToEpoch()
			});

			await Database.SaveChangesAsync().ConfigureAwait(false);

			await $"You gave rep to {user.Mention}".QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("unrep"), Summary("Removes a rep")]
		[Ratelimit(20, 1, Measure.Minutes)]
		[Usage("<@0>")]
		public async Task RemoveRep([Remainder] IGuildUser user)
		{
			if (user is not null && (user.IsBot || user.IsWebhook))
			{
				await EmbedExtensions.FromError(DiscordUtilities.NoBotsString, Context).QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			if ((user is not null && user.Id == Context.User.Id) || user is null)
			{
				await "You can't modify your own reputation".QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);
			var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

			if (Database.Reputations.Any(x => x.Repee == user.Id && x.Reper == Context.User.Id))
			{
				Database.Reputations.Remove(Database.Reputations.FirstOrDefault(x => x.Reper == usr.Id && x.Repee == user.Id));

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await $"You gave rep to {user.Mention}".QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}

			await "You haven't given this person a reputation point.".QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("title"), Summary("Sets Title"), RequireDatabase]
		[Usage("The village thief")]
		public async Task SetTitle([Remainder] string title = null)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

			if (title is null)
			{
				usr.Title = "";

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromSuccess("Successfully cleared your title.", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
			else
			{
				usr.Title = title;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromSuccess($"Successfully set your title to **{title}**", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[Name("Blocking"), Group("block")]
		[Remarks("🛑 Block actions")]
		public class Block : InteractiveBase<ShardedCommandContext>
		{
			[Command("recurring"), Summary("Blocks people from patting you on recurring digits"), RequireDatabase]
			public async Task BlockRecurring()
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

				usr.RecurringBlock = !usr.RecurringBlock;

				await Database.SaveChangesAsync().ConfigureAwait(false);

				await EmbedExtensions.FromSuccess($"Set RecurringBlock to: {usr.RecurringBlock}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
			}

			[Command("actions"), Summary("Blocks people from performing actions"), RequireDatabase]
			[Usage("<@0>")]
			public async Task BlockActions([Remainder] IUser user)
			{
				using var database = new SkuldDbContextFactory().CreateDbContext();

				var res = database.BlockedActions.ToList().FirstOrDefault(x => x.Blocker == Context.User.Id && x.Blockee == user.Id);

				if (res is not null)
				{
					database.BlockedActions.Remove(res);

					await EmbedExtensions.FromSuccess($"Unblocked {user.Mention}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}
				else
				{
					database.BlockedActions.Add(new BlockedAction
					{
						Blocker = Context.User.Id,
						Blockee = user.Id
					});

					await EmbedExtensions.FromSuccess($"Blocked {user.Mention}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
				}

				await database.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		[Name("Background"), Group("background")]
		[Remarks("🖼 Set your profile background")]
		public class CustomBG : InteractiveBase<ShardedCommandContext>
		{
			public SkuldConfig Configuration { get; set; }
			public ISkuldAPIClient ApiClient { get; set; }

			[
				Command("buy"),
				Summary("Buy permanent custom backgrounds"),
				RequireDatabase
			]
			public async Task BuyCBG()
			{
				using var Database = new SkuldDbContextFactory()
					.CreateDbContext();

				var usr = await Database.InsertOrGetUserAsync(Context.User)
					.ConfigureAwait(false);

				if (usr.UnlockedCustBG)
				{
					var gld = await Database.InsertOrGetGuildAsync(Context.Guild)
						.ConfigureAwait(false);

					await
						EmbedExtensions.FromInfo(
							"You already unlocked custom backgrounds, use: " +
							$"{gld.Prefix ?? Configuration.Prefix}" +
							"set-custombg [URL] to set your background",
							Context
						)
						.QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
				else
				{
					TransactionService.DoTransaction(
						new TransactionStruct
						{
							Amount = 40000,
							Sender = usr
						})
						.IsSuccessAsync(async x =>
						{
							using var Database = new SkuldDbContextFactory()
								.CreateDbContext();

							var usr = await Database.InsertOrGetUserAsync(Context.User)
								.ConfigureAwait(false);
							var gld = await Database.InsertOrGetGuildAsync(Context.Guild)
								.ConfigureAwait(false);

							usr.UnlockedCustBG = true;

							await Database.SaveChangesAsync().ConfigureAwait(false);

							await
								EmbedExtensions
									.FromSuccess(
										"You've successfully unlocked custom " +
										"backgrounds, use: " +
										$"{gld.Prefix ?? Configuration.Prefix}" +
										"set-custombg [URL] to set your background",
										Context
									)
								.QueueMessageAsync(Context)
							.ConfigureAwait(false);
						})
						.IsErrorAsync(async x =>
						{
							using var Database = new SkuldDbContextFactory()
								.CreateDbContext();

							var gld = await Database.InsertOrGetGuildAsync(Context.Guild)
								.ConfigureAwait(false);

							x.Exception.Is<TransactionException>().Then(
								async z =>
								{
									await
										EmbedExtensions
											.FromError(
											$"You need at least {gld.MoneyIcon}" +
											"40,000 to unlock custom backgrounds",
												Context
											)
										.QueueMessageAsync(Context)
									.ConfigureAwait(false);
								}
							);
						}
					);
				}
			}

			[Command("set"), Summary("Sets your custom background Image"), RequireDatabase]
			[Usage("https://example.com/SuperAwesomeImage.png", "#ff0000")]
			public async Task SetCBG(string link = null)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

				if (Uri.TryCreate(link, UriKind.Absolute, out var res))
				{
					TransactionService.DoTransaction(new TransactionStruct
					{
						Amount = 900,
						Sender = usr
					})
						.IsSuccess(async x =>
						{
							using var Database = new SkuldDbContextFactory().CreateDbContext();

							var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

							usr.Background = res.OriginalString;

							await Database.SaveChangesAsync().ConfigureAwait(false);

							await EmbedExtensions.FromSuccess("Set your Background", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						})
						.IsError(x =>
						{
							x.Exception.Is<TransactionException>().Then(async x =>
							{
								using var Database = new SkuldDbContextFactory().CreateDbContext();

								var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

								await EmbedExtensions.FromError($"You need at least {gld.MoneyIcon}900 to change your background", Context).QueueMessageAsync(Context).ConfigureAwait(false);
							});
						});
					return;
				}
				else if (link.ToLowerInvariant() == "reset")
				{
					usr.Background = "#3F51B5";

					await Database.SaveChangesAsync().ConfigureAwait(false);

					await EmbedExtensions.FromSuccess($"Reset your background to: {usr.Background}", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}
				else if (int.TryParse(link[0] != '#' ? link : link.Remove(0, 1), System.Globalization.NumberStyles.HexNumber, null, out _))
				{
					TransactionService.DoTransaction(new TransactionStruct
					{
						Amount = 300,
						Sender = usr
					}).IsSuccess(async x =>
					{
						using var Database = new SkuldDbContextFactory().CreateDbContext();

						var usr = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

						usr.Background = link[0] != '#' ? "#" + link : link;

						await Database.SaveChangesAsync().ConfigureAwait(false);

						await EmbedExtensions.FromSuccess("Set your Background", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					}).IsError(x =>
					{
						x.Exception.Is<TransactionException>().Then(async x =>
						{
							using var Database = new SkuldDbContextFactory().CreateDbContext();

							var gld = await Database.InsertOrGetGuildAsync(Context.Guild).ConfigureAwait(false);

							await EmbedExtensions.FromError($"You need at least {gld.MoneyIcon}300 to change your background", Context).QueueMessageAsync(Context).ConfigureAwait(false);
						});
					});
				}
				else
				{
					await EmbedExtensions.FromError($"Malformed Entry", Context).QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}
			}

			[Command("preview"), Summary("Previews a custom bg")]
			[Ratelimit(20, 1, Measure.Minutes)]
			[Usage("https://example.com/SuperAwesomeImage.png", "#445566")]
			public async Task PreviewCustomBG(string link)
			{
				var image = await ApiClient.GetExampleProfileCardAsync(Context.User.Id, link);

				await "".QueueMessageAsync(Context, filestream: image, filename: "image.png").ConfigureAwait(false);
			}
		}

		[Command("settimezone"), Summary("Sets your timezone")]
		[Usage("UTC")]
		public async Task SetTimeZone([Remainder] DateTimeZone timezone)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var user = await Database.InsertOrGetUserAsync(Context.User).ConfigureAwait(false);

			user.TimeZone = timezone.Id;

			await Database.SaveChangesAsync().ConfigureAwait(false);

			await
				new EmbedBuilder()
				.AddFooter(Context)
				.AddAuthor()
				.WithDescription($"Set your timezone to: {timezone.Id}")
			.QueueMessageAsync(Context).ConfigureAwait(false);
		}
	}
}