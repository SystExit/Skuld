﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Skuld.Bot.Discord.Models;
using Skuld.Bot.Discord.Preconditions;
using Skuld.Bot.Extensions;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Formatting;
using Skuld.Core.Extensions.Verification;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Models.Commands;
using Skuld.Services.Accounts.Banking.Models;
using Skuld.Services.Accounts.Experience;
using Skuld.Services.Banking;
using Skuld.Services.BotListing;
using Skuld.Services.Extensions;
using Skuld.Services.Messaging.Extensions;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skuld.Bot.Modules
{
	[Group, Name("Developer")]
	[RequireBotFlag(BotAccessLevel.BotAdmin)]
	[Remarks("👩‍💻 Dev Only")]
	public class DeveloperModule : ModuleBase<ShardedCommandContext>
	{
		public SkuldConfig Configuration { get; set; }

		#region BotAdmin

		[Command("bean")]
		public async Task Bean(IGuildUser user, [Remainder] string reason = null)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var usr = Database.Users.Find(user.Id);

			if (usr.Flags.IsBitSet(DiscordUtilities.Banned))
			{
				usr.Flags -= DiscordUtilities.Banned;
				usr.BanReason = null;
				await
					EmbedExtensions.FromSuccess(SkuldAppContext.GetCaller(), $"Un-beaned {user.Mention}", Context)
				.QueueMessageAsync(Context).ConfigureAwait(false);
			}
			else
			{
				if (reason is null)
				{
					await
						EmbedExtensions.FromError($"{nameof(reason)} needs a value", Context)
					.QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}
				usr.Flags += DiscordUtilities.Banned;
				usr.BanReason = reason;
				await
					EmbedExtensions.FromSuccess(SkuldAppContext.GetCaller(), $"Beaned {user.Mention} for reason: `{reason}`", Context)
				.QueueMessageAsync(Context).ConfigureAwait(false);
			}

			await Database.SaveChangesAsync().ConfigureAwait(false);
		}

		[Command("shardrestart"), Summary("Restarts shard")]
		public async Task ReShard(int shard)
		{
			await Context.Client.GetShard(shard).StopAsync().ConfigureAwait(false);
			await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
			await Context.Client.GetShard(shard).StartAsync().ConfigureAwait(false);
		}

		[Command("dumpshards"), Summary("Shard Info"), Alias("dumpshard")]
		public async Task Shard()
		{
			var lines = new List<string[]>
			{
				new string[] { "Shard", "State", "Latency", "Guilds" }
			};
			foreach (var item in Context.Client.Shards)
			{
				lines.Add(new string[] {
					Convert.ToString(item.ShardId, CultureInfo.InvariantCulture),
					Convert.ToString(item.ConnectionState, CultureInfo.InvariantCulture),
					Convert.ToString(item.Latency, CultureInfo.InvariantCulture),
					Convert.ToString(item.Guilds, CultureInfo.InvariantCulture)
				});
			}

			await $"```\n{lines.PrettyLines(2)}```".QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("getshard"), Summary("Gets all information about specific shard")]
		public async Task ShardGet(int shardid = -1)
		{
			if (shardid > -1)
				await ShardInfo(shardid).ConfigureAwait(false);
			else
				await ShardInfo(Context.Client.GetShardIdFor(Context.Guild)).ConfigureAwait(false);
		}

		public async Task ShardInfo(int shardid)
		{
			var shard = Context.Client.GetShard(shardid);
			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = $"Shard ID: {shardid}",
					IconUrl = shard.CurrentUser.GetAvatarUrl()
				},
				Footer = new EmbedFooterBuilder
				{
					Text = "Generated at"
				},
				Timestamp = DateTime.Now,
				Color = EmbedExtensions.RandomEmbedColor()
			};
			embed.AddInlineField("Guilds", Convert.ToString(shard.Guilds.Count, CultureInfo.InvariantCulture));
			embed.AddInlineField("Status", shard.ConnectionState);
			embed.AddInlineField("Latency", shard.Latency + "ms");
			embed.AddField("Game", shard.Activity.Name);
			await embed.Build().QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("shard"), Summary("Gets the shard the guild is on")]
		public async Task ShardGet(ulong guildId = 0)
		{
			IGuild guild;
			if (guildId != 0)
				guild = SkuldApp.DiscordClient.GetGuild(guildId);
			else
				guild = Context.Guild;

			await $"{Context.User.Mention} the server: `{guild.Name}` is on `{Context.Client.GetShardIdFor(guild)}`".QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("setgame"), Summary("Set Game")]
		public async Task Game([Remainder] string title)
		{
			await Context.Client.SetGameAsync(title).ConfigureAwait(false);
		}

		[Command("resetgame"), Summary("Reset Game")]
		public async Task ResetGame()
		{
			foreach (var shard in Context.Client.Shards)
			{
				await shard.SetGameAsync($"{Configuration.Prefix}help | {shard.ShardId + 1}/{Context.Client.Shards.Count}", type: ActivityType.Listening).ConfigureAwait(false);
			}
		}

		[Command("setstream"), Summary("Sets stream")]
		public async Task Stream(string streamer, [Remainder] string title)
		{
			await Context.Client.SetGameAsync(title, "https://twitch.tv/" + streamer, ActivityType.Streaming).ConfigureAwait(false);
		}

		[Command("setactivity"), Summary("Sets activity")]
		public async Task ActivityAsync(ActivityType activityType, [Remainder] string status)
		{
			await Context.Client.SetGameAsync(status, null, activityType).ConfigureAwait(false);
		}

		[Command("grantxp"), Summary("Grant Exp")]
		public async Task GrantExp(ulong amount, [Remainder] IGuildUser user = null)
		{
			if (user is null)
				user = Context.Guild.GetUser(Context.User.Id);

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var usr = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

			await usr.GrantExperienceAsync(amount, Context, ExperienceService.DefaultAction, true).ConfigureAwait(false);

			await EmbedExtensions.FromSuccess($"Gave {user.Mention} {amount.ToFormattedString()}xp", Context).QueueMessageAsync(Context).ConfigureAwait(false);
		}

		#endregion BotAdmin

		#region BotOwner

		[Command("stop")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task Stop()
		{
			await "Stopping!".QueueMessageAsync(Context).ConfigureAwait(false);
			await SkuldApp.StopBotAsync("StopCmd").ConfigureAwait(false);
		}

		[Command("setflag")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task SetFlag(BotAccessLevel level, bool give = true, [Remainder] IUser user = null)
		{
			if (user is null)
			{
				user = Context.User;
			}

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var dbUser = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

			bool DidAny = false;

			switch (level)
			{
				case BotAccessLevel.BotOwner:
					if (give & !dbUser.Flags.IsBitSet(DiscordUtilities.BotCreator))
					{
						dbUser.Flags += DiscordUtilities.BotCreator;
						DidAny = true;
					}
					else if (!give && dbUser.Flags.IsBitSet(DiscordUtilities.BotCreator))
					{
						dbUser.Flags -= DiscordUtilities.BotCreator;
						DidAny = true;
					}
					break;

				case BotAccessLevel.BotAdmin:
					if (give && !dbUser.Flags.IsBitSet(DiscordUtilities.BotAdmin))
					{
						dbUser.Flags += DiscordUtilities.BotAdmin;
						DidAny = true;
					}
					else if (!give && dbUser.Flags.IsBitSet(DiscordUtilities.BotAdmin))
					{
						dbUser.Flags -= DiscordUtilities.BotAdmin;
						DidAny = true;
					}
					break;

				case BotAccessLevel.BotTester:
					if (give && !dbUser.Flags.IsBitSet(DiscordUtilities.BotTester))
					{
						dbUser.Flags += DiscordUtilities.BotTester;
						DidAny = true;
					}
					else if (!give && dbUser.Flags.IsBitSet(DiscordUtilities.BotTester))
					{
						dbUser.Flags -= DiscordUtilities.BotTester;
						DidAny = true;
					}
					break;

				case BotAccessLevel.BotDonator:
					if (give && !dbUser.IsDonator)
					{
						dbUser.Flags += DiscordUtilities.BotDonator;
						DidAny = true;
					}
					else if (!give && dbUser.IsDonator)
					{
						dbUser.Flags -= DiscordUtilities.BotDonator;
						DidAny = true;
					}
					break;
			}

			if (DidAny)
			{
				await Database.SaveChangesAsync().ConfigureAwait(false);
			}

			if (DidAny)
			{
				await $"{(give ? "Added" : "Removed")} flag `{level}` to {user.Mention}".QueueMessageAsync(Context).ConfigureAwait(false);
			}
			else
			{
				await $"{user.Mention} {(give ? "already has" : "doesn't have")} the flag `{level}`".QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[Command("flags")]
		public async Task GetFlags([Remainder] IUser user = null)
		{
			if (user is null)
			{
				user = Context.User;
			}

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var dbUser = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

			List<BotAccessLevel> flags = new();

			if (dbUser.Flags.IsBitSet(DiscordUtilities.BotCreator))
			{
				flags.Add(BotAccessLevel.BotOwner);
			}
			if (dbUser.Flags.IsBitSet(DiscordUtilities.BotAdmin))
			{
				flags.Add(BotAccessLevel.BotAdmin);
			}
			if (!dbUser.IsDonator)
			{
				flags.Add(BotAccessLevel.BotDonator);
			}
			if (dbUser.Flags.IsBitSet(DiscordUtilities.BotTester))
			{
				flags.Add(BotAccessLevel.BotTester);
			}

			flags.Add(BotAccessLevel.Normal);

			await $"{user.Mention} has the flags `{string.Join(", ", flags)}`".QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("jsoncommands")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task Commands()
		{
			var Modules = new List<ModuleSkuld>();

			SkuldApp.CommandService.Modules.ToList().ForEach(module =>
			{
				if (!module.Attributes.Any(x => x.GetType() == typeof(DisabledAttribute)))
				{
					ModuleSkuld mod = new()
					{
						Name = module.GetTopLevelParent().Name,
						ModulePath = module.GetModulePath(),
						Commands = new List<CommandSkuld>()
					};
					module.Commands.ToList().ForEach(cmd =>
					{
						if (!cmd.Attributes.Any(x => x.GetType() == typeof(DisabledAttribute)))
						{
							var parameters = new List<ParameterSkuld>();

							cmd.Parameters.ToList().ForEach(paras =>
							{
								parameters.Add(new ParameterSkuld
								{
									Name = paras.Name,
									Optional = paras.IsOptional
								});
							});

							mod.Commands.Add(new CommandSkuld
							{
								Name = cmd.Name,
								Description = cmd.Summary,
								Aliases = cmd.Aliases.ToArray(),
								Parameters = parameters.Any() ? parameters.ToArray() : null
							});
						}
					});
					Modules.Add(mod);
				}
			});

			var filename = Path.Combine(SkuldAppContext.StorageDirectory, "commands.json");

			File.WriteAllText(filename, JsonConvert.SerializeObject(Modules));

			await $"Written commands to {filename}".QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("name"), Summary("Name")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task Name([Remainder] string name)
			=> await Context.Client.CurrentUser.ModifyAsync(x => x.Username = name).ConfigureAwait(false);

		[Command("status"), Summary("Status")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task Status(UserStatus status)
		{
			await Context.Client.SetStatusAsync(status).ConfigureAwait(false);
		}

		[Command("debug"), Summary("Get Debug stuff")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task Debug()
		{
			using MemoryStream stream = new();
			using TextWriter writer = new StreamWriter(stream);

			await writer.WriteLineAsync($"SKULD VERSION {Assembly.GetExecutingAssembly().GetName().Version}");

			await writer.WriteLineAsync($"\tINFORMATION");

			await writer.WriteLineAsync($"\t\tRUNTIME");
			await writer.WriteLineAsync($"\t\t\tSTARTED {Process.GetCurrentProcess().StartTime.ToDMYString()}");
			await writer.WriteLineAsync($"\t\t\tUPTIME {DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime).ToDifferenceString()}");
			await writer.WriteLineAsync($"\t\t\tCOMMANDS: {SkuldApp.CommandService.Commands.Count()}");
			await writer.WriteLineAsync($"\t\t\tMODULES: {SkuldApp.CommandService.Modules.Count()}");
			await writer.WriteLineAsync($"\t\t\tMEMORY USAGE: {SkuldAppContext.MemoryStats.GetMBUsage}MB");
			await writer.WriteLineAsync($"\t\t\tCPU Usage: {await SkuldAppContext.GetCurrentCPUUsage()}%");

			await writer.WriteLineAsync($"\t\tDEPENDENCIES");

			foreach (AssemblyName dependent in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
			{
				await writer.WriteLineAsync($"\t\t\t{dependent.Name} VERSION {dependent.Version}");
			}

			await writer.FlushAsync();

			stream.Position = 0;

			await Context.User.SendFileAsync(stream, "debug.txt");
		}

		[Command("dropuser")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task DropUser(ulong userId)
		{
			using var database = new SkuldDbContextFactory().CreateDbContext();

			await database.DropUserAsync(userId).ConfigureAwait(false);

			await database.SaveChangesAsync().ConfigureAwait(false);
		}

		[Command("dropguild")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task DropGuild(ulong guildId)
		{
			using var database = new SkuldDbContextFactory().CreateDbContext();

			await database.DropGuildAsync(guildId).ConfigureAwait(false);

			await database.SaveChangesAsync().ConfigureAwait(false);
		}

		[Command("merge")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task Merge(ulong oldId, ulong newId)
		{
			if (Context.Client.GetUser(newId) is null)
			{
				await $"No. {newId} is not a valid user Id"
					.QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}
			if (newId == oldId)
			{
				await $"No.".QueueMessageAsync(Context).ConfigureAwait(false);
				return;
			}
			try
			{
				//UserAccount
				{
					using var db = new SkuldDbContextFactory()
						.CreateDbContext();

					var oldUser = db.Users.Find(oldId);
					var newUser = db.Users.Find(newId);

					if (oldUser is not null && newUser is not null)
					{
						TransactionService.DoTransaction(new TransactionStruct
						{
							Amount = oldUser.Money,
							Receiver = newUser
						});
						newUser.Title = oldUser.Title;
						newUser.Language = oldUser.Language;
						newUser.Patted = oldUser.Patted;
						newUser.Pats = oldUser.Pats;
						newUser.UnlockedCustBG = oldUser.UnlockedCustBG;
						newUser.Background = oldUser.Background;

						await db.SaveChangesAsync().ConfigureAwait(false);
					}
				}

				//Reputation
				{
					using var db = new SkuldDbContextFactory()
						.CreateDbContext();

					var repee = db.Reputations.AsQueryable()
						.Where(x => x.Repee == oldId);
					var reper = db.Reputations.AsQueryable()
						.Where(x => x.Reper == oldId);

					if (repee.Any())
					{
						foreach (var rep in repee)
						{
							if (!db.Reputations
								.Any(x =>
										x.Repee == newId &&
										x.Reper == rep.Reper
									)
							)
							{
								rep.Repee = newId;
							}
						}
					}

					if (reper.Any())
					{
						foreach (var rep in reper)
						{
							if (!db.Reputations
								.Any(x =>
										x.Reper == newId &&
										x.Repee == rep.Repee
									)
							)
							{
								rep.Reper = newId;
							}
						}
					}

					if (repee.Any() || reper.Any())
					{
						await db.SaveChangesAsync().ConfigureAwait(false);
					}
				}

				//Pastas
				{
					using var db = new SkuldDbContextFactory()
						.CreateDbContext();

					var pastas = db.Pastas.AsQueryable()
						.Where(x => x.OwnerId == oldId);

					if (pastas.Any())
					{
						foreach (var pasta in pastas)
						{
							pasta.OwnerId = newId;
						}

						await db.SaveChangesAsync().ConfigureAwait(false);
					}
				}

				//PastaVotes
				{
					using var db = new SkuldDbContextFactory()
						.CreateDbContext();

					var pastaVotes = db.PastaVotes.AsQueryable()
						.Where(x => x.VoterId == oldId);

					if (pastaVotes.Any())
					{
						foreach (var pasta in pastaVotes)
						{
							pasta.VoterId = newId;
						}
					}

					await db.SaveChangesAsync().ConfigureAwait(false);
				}

				//CommandUsage
				{
					using var db = new SkuldDbContextFactory()
						.CreateDbContext();

					var commands = db.UserCommandUsage.AsQueryable()
						.Where(x => x.UserId == oldId);

					if (commands.Any())
					{
						foreach (var command in commands)
						{
							command.UserId = newId;
						}

						await db.SaveChangesAsync().ConfigureAwait(false);
					}
				}

				//Experience
				{
					using var db = new SkuldDbContextFactory()
						.CreateDbContext();

					var experiences = db.UserXp.AsQueryable()
						.Where(x => x.UserId == oldId);

					var newExperiences = db.UserXp.AsQueryable()
						.Where(x => x.UserId == newId);

					if (experiences.Any() && !newExperiences.Any())
					{
						foreach (var experience in experiences)
						{
							experience.UserId = newId;
						}
					}
					else if (experiences.Any() && newExperiences.Any())
					{
						foreach (var experience in experiences)
						{
							if (newExperiences
								.Any(x =>
									x.GuildId == experience.GuildId
									)
							)
							{
								var xp = newExperiences
									.FirstOrDefault(x =>
										x.GuildId == experience.GuildId
									);

								xp.TotalXP = xp.TotalXP
									.Add(experience.TotalXP);
							}
						}
					}

					await db.SaveChangesAsync().ConfigureAwait(false);
				}

				//Prune Old User
				{
					using var db = new SkuldDbContextFactory().CreateDbContext();

					await db.DropUserAsync(oldId).ConfigureAwait(false);
				}

				await $"Successfully merged data from {oldId} into {newId}".QueueMessageAsync(Context).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Error("MergeCmd", ex.Message, Context, ex);
				await "Check the console log".QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		[Command("generatekeys")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		[RequireContext(ContextType.DM)]
		public async Task CreateKeys(ulong amount)
		{
			using var db = new SkuldDbContextFactory().CreateDbContext();

			StringBuilder message = new($"Keycodes:");

			message.AppendLine();

			for (ulong x = 0; x < amount; x++)
			{
				var key = new DonatorKey
				{
					KeyCode = Guid.NewGuid()
				};
				db.DonatorKeys.Add(key);

				message.AppendLine($"Keycode: {key.KeyCode}");
			}

			DogStatsd.Increment("donatorkeys.generated", (int)amount);

			await db.SaveChangesAsync().ConfigureAwait(false);

			await message.QueueMessageAsync(
				Context,
				type: Services.Messaging.Models.MessageType.DMS
			).ConfigureAwait(false);
		}

		[Command("resetdaily")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task ResetDaily([Remainder] IUser user = null)
		{
			if (user is null)
				user = Context.User;

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var usr = await Database.InsertOrGetUserAsync(user)
				.ConfigureAwait(false);

			usr.LastDaily = 0;

			await Database.SaveChangesAsync().ConfigureAwait(false);

			await
				EmbedExtensions.FromSuccess(Context).QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[Command("setstreak")]
		public async Task SetStreak(ushort streak, [Remainder] IUser user = null)
		{
			if (user is null)
				user = Context.User;

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var usr = await Database.InsertOrGetUserAsync(user)
				.ConfigureAwait(false);

			usr.Streak = streak;

			await Database.SaveChangesAsync().ConfigureAwait(false);

			await
				EmbedExtensions.FromSuccess(Context).QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[Command("donatorstatus")]
		public async Task CheckDonatorStatus(IUser user)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var keys = Database.DonatorKeys.ToList().Where(x => x.Redeemer == user.Id);

			StringBuilder message = new();

			message.Append(user.Mention)
				.Append(" is ");

			if (keys.Any())
			{
				var keysordered = keys.OrderBy(x => x.RedeemedWhen);

				var amount = 365 * keys.Count();

				var time = keysordered.LastOrDefault().RedeemedWhen.FromEpoch();

				time = time.AddDays(amount);

				message.Append("a donator until ")
					.Append(time.ToDMYString());
			}
			else
			{
				message.Append("not a donator 🙁");
			}

			await message.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("checkkey")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		[RequireContext(ContextType.DM)]
		public async Task CheckKey(Guid key)
		{
			using var db = new SkuldDbContextFactory().CreateDbContext();

			var donorkey = db.DonatorKeys
				.FirstOrDefault(x => x.KeyCode == key);

			StringBuilder message = new();

			message
				.Append("Key: `")
				.Append(key)
				.Append("` ");

			if (donorkey is not null)
			{
				message
					.Append("is")
					.Append(donorkey.Redeemed ? "" : " not ")
					.Append("redeemed");
			}
			else
			{
				message.Append("doesn't exist");
			}

			await message.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[
			Command("moneyadd"),
			Summary("Gives money to people"),
			RequireDatabase
		]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task GiveMoney(IGuildUser user, long amount)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var usr = await Database.InsertOrGetUserAsync(user)
				.ConfigureAwait(false);

			if (amount < 0)
			{
				TransactionService.DoTransaction(new TransactionStruct
				{
					Amount = (ulong)Math.Abs(amount),
					Receiver = usr
				});
			}
			else
			{
				TransactionService.DoTransaction(new TransactionStruct
				{
					Amount = (ulong)amount,
					Receiver = usr
				});
			}

			await Database.SaveChangesAsync().ConfigureAwait(false);

			var icon = (
				await Database.InsertOrGetGuildAsync(Context.Guild)
				.ConfigureAwait(false)
			).MoneyIcon;

			StringBuilder message = new();

			message
				.Append("User ")
				.Append(user.Username)
				.Append(" now has: ")
				.Append(icon)
				.Append(usr.Money.ToFormattedString());

			await message.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		[Command("setmoney")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task SetMoney(IGuildUser user, ulong amount)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var usr = await Database.InsertOrGetUserAsync(user).ConfigureAwait(false);

			usr.Money = amount;

			await Database.SaveChangesAsync().ConfigureAwait(false);

			await EmbedExtensions.FromSuccess(
				$"Set money of {user.Mention} to {usr.Money}",
				Context).QueueMessageAsync(Context)
			.ConfigureAwait(false);
		}

		[Command("leave"), Summary("Leaves a server by id")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task LeaveServer(ulong id)
		{
			var guild = Context.Client.GetGuild(id);
			await guild.LeaveAsync().ContinueWith(async x =>
			{
				if (Context.Client.GetGuild(id) is null)
				{
					await EmbedExtensions.FromSuccess(
						$"Left guild **{guild.Name}**",
						Context).QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
				else
				{
					await EmbedExtensions.FromError(
						$"Hmm, I haven't left **{guild.Name}**",
						Context).QueueMessageAsync(Context)
					.ConfigureAwait(false);
				}
			}).ConfigureAwait(false);
		}

		[Command("pubstats"), Summary("no")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task PubStats()
		{
			await
				"Ok, publishing stats to the Discord Bot lists."
				.QueueMessageAsync(Context)
			.ConfigureAwait(false);

			StringBuilder list = new();
			int shardcount = Context.Client.Shards.Count;

			await Context.Client.SendDataAsync(
				Configuration.IsDevelopmentBuild,
				Configuration.DiscordGGKey,
				Configuration.DBotsOrgKey,
				Configuration.B4DToken)
			.ConfigureAwait(false);

			foreach (var shard in Context.Client.Shards)
			{
				list
					.Append("I sent ShardID: ")
					.Append(shard.ShardId)
					.Append(" Guilds: ")
					.Append(shard.Guilds.Count)
					.Append(" Shards: ")
					.AppendLine(Convert.ToString(shardcount, CultureInfo.InvariantCulture));
			}

			await list.QueueMessageAsync(Context).ConfigureAwait(false);
		}

		public static Globals evalGlobals = new();
		public static ScriptOptions scriptOptions = null;

		[Command("eval"), Summary("no")]
		[RequireBotFlag(BotAccessLevel.BotOwner)]
		public async Task EvalStuff([Remainder] string code)
		{
			evalGlobals.Context = Context as ShardedCommandContext;

			if (scriptOptions is null)
			{
				scriptOptions = ScriptOptions
				.Default
				.WithReferences(typeof(SkuldDbContext).Assembly)
				.WithReferences(
							 typeof(ShardedCommandContext).Assembly,
							 typeof(ShardedCommandContext).Assembly,
							 typeof(SocketGuildUser).Assembly,
							 typeof(Task).Assembly,
							 typeof(Queryable).Assembly,
							 typeof(SkuldApp).Assembly
				)
				.WithImports(typeof(SkuldDbContext).FullName)
				.WithImports(typeof(ShardedCommandContext).FullName,
							 typeof(ShardedCommandContext).FullName,
							 typeof(SocketGuildUser).FullName,
							 typeof(Task).FullName,
							 typeof(Queryable).FullName,
							 typeof(SkuldApp).FullName
				);
			}

			try
			{
				if (code.ToLowerInvariant().Contains("token") ||
					code.ToLowerInvariant().Contains("key") ||
					code.ToLowerInvariant().Contains("configuration"))
				{
					await EmbedExtensions.FromError(
						"Script Evaluation",
						"Nope.",
						Context)
					.QueueMessageAsync(Context).ConfigureAwait(false);
					return;
				}

				if ((code.StartsWith("```cs", StringComparison.Ordinal) ||
					code.StartsWith("```", StringComparison.Ordinal)) &&
					!code.EndsWith("```", StringComparison.Ordinal))
				{
					await
						EmbedExtensions.FromError(
							"Script Evaluation",
							"Starting Codeblock tags does not " +
							"finish with closing Codeblock tags",
							Context
						).QueueMessageAsync(Context)
					.ConfigureAwait(false);
					return;
				}

				if (code.StartsWith("```cs", StringComparison.Ordinal))
				{
					code = code.ReplaceFirst("```cs", "");
					code = code.ReplaceLast("```", "");
				}
				else if (code.StartsWith("```", StringComparison.Ordinal))
				{
					code = code.Replace("```", "");
				}

				var script = CSharpScript.Create(
					code,
					scriptOptions,
					typeof(Globals));

				script.Compile();

				var execution = await
					script.RunAsync(globals: evalGlobals)
				.ConfigureAwait(false);

				var result = execution.ReturnValue;

				string type = "N/A";

				if (result is not null)
				{
					type = result.GetType().ToString();
				}

				StringBuilder evalDesc = new();

				evalDesc
					.Append("__**Execution Result**__")
					.AppendLine()
					.Append("Returned Type: ")
					.AppendLine(type)
					.Append("Value: ")
					.Append(result);

				await EmbedExtensions.FromMessage(
					"Script Evaluation",
					evalDesc.ToString(),
					Context
				).QueueMessageAsync(Context).ConfigureAwait(false);
			}
#pragma warning disable CS0168 // Variable is declared but never used
			catch (NullReferenceException ex) { /*Do nothing here*/ }
#pragma warning restore CS0168 // Variable is declared but never used
			catch (Exception ex)
			{
				Log.Error(
					"EvalCMD",
					"Error with eval command " + ex.Message,
					Context,
					ex);

				await EmbedExtensions.FromError(
					"Script Evaluation",
					$"Error with eval command\n\n{ex.Message}",
					Context
				).QueueMessageAsync(Context).ConfigureAwait(false);
			}
		}

		public class Globals
		{
			public ShardedCommandContext Context { get; set; }
		}

		#endregion BotOwner
	}
}