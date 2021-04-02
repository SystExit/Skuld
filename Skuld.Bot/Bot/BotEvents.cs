using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Skuld.API;
using Skuld.Core;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Verification;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Services.BotListing;
using Skuld.Services.Guilds.Pinning;
using Skuld.Services.Guilds.Starboard;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using User = Skuld.Models.User;

namespace Skuld.Bot
{
	internal static class BotEvents
	{
		public const string Key = "DiscordLog";
		private static readonly List<int> ShardsReady = new();
		static DiscordShardedClient Client => SkuldApp.DiscordClient;
		static readonly List<Tuple<string, ActivityType>> Statuses = new()
		{
			new Tuple<string, ActivityType>("{{PREFIX}}help | {{SHARDID}}/{{SHARDCOUNT}}", ActivityType.Listening),
			new Tuple<string, ActivityType>("with {{TOTALSERVERCOUNT}} servers", ActivityType.Playing),
			new Tuple<string, ActivityType>(SkuldAppContext.Website, ActivityType.Playing),
		};

		//DiscordLoging
		public static void RegisterEvents()
		{
			/*All Events needed for running Skuld*/
			Client.ShardConnected += Bot_ShardConnected;
			Client.ShardDisconnected += Bot_ShardDisconnected;
			Client.ShardReady += Bot_ShardReady;
			Client.MessageReceived += BotMessaging.HandleMessage;
		}

		public static void UnRegisterEvents()
		{
			Client.ShardConnected -= Bot_ShardConnected;
			Client.ShardDisconnected -= Bot_ShardDisconnected;
			Client.ShardReady -= Bot_ShardReady;
			Client.MessageReceived -= BotMessaging.HandleMessage;

			foreach (var shard in Client.Shards)
			{
				shard.MessagesBulkDeleted -= Shard_MessagesBulkDeleted;
				shard.MessageDeleted -= Shard_MessageDeleted;
				shard.JoinedGuild -= Bot_JoinedGuild;
				shard.RoleDeleted -= Bot_RoleDeleted;
				shard.GuildMemberUpdated -= Bot_GuildMemberUpdated;
				shard.LeftGuild -= Bot_LeftGuild;
				shard.UserJoined -= Bot_UserJoined;
				shard.UserLeft -= Bot_UserLeft;
				shard.ReactionAdded -= Bot_ReactionAdded;
				shard.ReactionRemoved -= Bot_ReactionRemoved;
				shard.ReactionsCleared -= Bot_ReactionsCleared;
				shard.Log -= Bot_Log;
				shard.UserUpdated -= Bot_UserUpdated;
				shard.GuildUpdated -= Bot_GuildUpdated;
			}
		}

		private static Task Bot_Log(
			LogMessage arg
		)
		{
			var key = $"{Key} - {arg.Source}";
			switch (arg.Severity)
			{
				case LogSeverity.Info:
					Log.Info(key, arg.Message);
					break;

				case LogSeverity.Critical:
					Log.Critical(key, arg.Message, null, arg.Exception);
					break;

				case LogSeverity.Warning:
					Log.Warning(key, arg.Message, null, arg.Exception);
					break;

				case LogSeverity.Verbose:
					Log.Verbose(key, arg.Message, null, arg.Exception);
					break;

				case LogSeverity.Error:
					Log.Error(key, arg.Message, null, arg.Exception);
					break;

				case LogSeverity.Debug:
					Log.Debug(key, arg.Message, null, arg.Exception);
					break;

				default:
					break;
			}
			return Task.CompletedTask;
		}

		#region Reactions

		private static async Task Bot_ReactionAdded(
			Cacheable<IUserMessage, ulong> arg1,
			ISocketMessageChannel arg2,
			SocketReaction arg3)
		{
			DogStatsd.Increment("messages.reactions.added");
			IUser usr;

			if (!arg3.User.IsSpecified) return;
			else
			{
				usr = arg3.User.Value;
			}

			if (usr.IsBot || usr.IsWebhook) return;

			var msg = await arg1.GetOrDownloadAsync().ConfigureAwait(false);

			if (msg is null) return;

			if (arg2 is IGuildChannel)
			{
				await PinningService.ExecuteAdditionAsync(SkuldApp.DiscordClient, SkuldApp.Configuration, msg, arg2)
					.ConfigureAwait(false);

				await StarboardService.ExecuteAdditionAsync(msg, arg2, arg3)
					.ConfigureAwait(false);
			}

			if (arg2.Id == SkuldApp.Configuration.IssueChannel)
			{
				using var Database = new SkuldDbContextFactory()
					.CreateDbContext();

				var user = await Database.InsertOrGetUserAsync(usr)
					.ConfigureAwait(false);

				if (user.Flags.IsBitSet(DiscordUtilities.BotCreator))
				{
					try
					{
						if (arg1.HasValue)
						{
							var message = Database.Issues
								.FirstOrDefault(x =>
									x.IssueChannelMessageId == arg1.Id
								);
							if (message is not null)
							{
								var emote = arg3.Emote as Emote;

								if (emote.Id == DiscordUtilities.Tick_Emote.Id)
								{
									if (!message.HasSent)
									{
										try
										{
											var newissue =
											new NewIssue(message.Title)
											{
												Body = message.Body
											};

											newissue.
												Assignees.Add("exsersewo");
											newissue.
												Labels.Add("From Command");

											var issue = await SkuldApp
												.Services
												.GetRequiredService
													<GitHubClient>()
												.Issue
												.Create(
													SkuldApp
														.Configuration
														.GithubRepository,
													newissue
												).ConfigureAwait(false);

											try
											{
												await SkuldApp.DiscordClient
													.GetUser(
														message.SubmitterId
													)
													.SendMessageAsync(
														"",
														false,
														new EmbedBuilder()
															.WithTitle(
																"Good News!"
															)
															.AddAuthor(
																SkuldApp
																.DiscordClient
															)
															.WithDescription(
													"Your issue:\n" +
													$"\"[{newissue.Title}]" +
													$"({issue.HtmlUrl})\"" +
													"\n\nhas been accepted"
															)
															.WithRandomColor()
														.Build()
												).ConfigureAwait(false);
											}
											catch { }

											await msg.ModifyAsync(x =>
											{
												x.Embed = msg.Embeds
												.ElementAt(0)
												.ToEmbedBuilder()
												.AddField(
													"Sent",
													DiscordUtilities
														.Tick_Emote
														.ToString()
												)
												.Build();
											}).ConfigureAwait(false);

											message.HasSent = true;

											await Database.SaveChangesAsync()
												.ConfigureAwait(false);
										}
										catch (Exception ex)
										{
											Log.Error(
												"Git-" +
												SkuldAppContext.GetCaller(),
												ex.Message,
												null,
												ex
											);
										}
									}
								}
								else if (
									emote.Id == DiscordUtilities.Cross_Emote.Id
								)
								{
									Database.Issues.Remove(message);

									await Database.SaveChangesAsync()
										.ConfigureAwait(false);

									await msg.DeleteAsync()
										.ConfigureAwait(false);

									try
									{
										await SkuldApp
											.DiscordClient
											.GetUser(message.SubmitterId)
											.SendMessageAsync(
												"",
												false,
												new EmbedBuilder()
													.WithTitle("Bad News")
													.AddAuthor(
													SkuldApp.DiscordClient
													)
													.WithDescription(
											"Your issue:\n" +
											$"\"{message.Title}\"" +
											"\n\nhas been declined. " +
											"If you would like to know why, " +
											$"send: {usr.FullName()} a message"
													)
													.WithRandomColor()
												.Build()
										).ConfigureAwait(false);
									}
									catch { }
								}
							}
						}
					}
					catch (Exception ex)
					{
						Log.Critical(Key,
							ex.Message,
							null,
							ex
						);
					}
				}
			}
		}

		private static Task Bot_ReactionsCleared(
			Cacheable<IUserMessage, ulong> arg1,
			ISocketMessageChannel arg2)
		{
			DogStatsd.Increment("messages.reactions.cleared");
			return Task.CompletedTask;
		}

		private static async Task Bot_ReactionRemoved(
			Cacheable<IUserMessage, ulong> arg1,
			ISocketMessageChannel arg2,
			SocketReaction arg3)
		{
			DogStatsd.Increment("messages.reactions.removed");

			IUser usr;

			if (!arg3.User.IsSpecified) return;
			else
			{
				usr = arg3.User.Value;
			}

			if (usr.IsBot || usr.IsWebhook) return;

			var message = await arg1.GetOrDownloadAsync()
				.ConfigureAwait(false);

			if (message is null) return;

			await StarboardService.ExecuteRemovalAsync(message, usr.Id)
				.ConfigureAwait(false);
		}

		#endregion Reactions

		#region Shards

		private static Task Bot_ShardReady(
			DiscordSocketClient arg
		)
		{
			if (!ShardsReady.Contains(arg.ShardId))
			{
				arg.MessageDeleted += Shard_MessageDeleted;
				arg.MessagesBulkDeleted += Shard_MessagesBulkDeleted;
				arg.JoinedGuild += Bot_JoinedGuild;
				arg.RoleDeleted += Bot_RoleDeleted;
				arg.GuildMemberUpdated += Bot_GuildMemberUpdated;
				arg.LeftGuild += Bot_LeftGuild;
				arg.UserJoined += Bot_UserJoined;
				arg.UserLeft += Bot_UserLeft;
				arg.ReactionAdded += Bot_ReactionAdded;
				arg.ReactionRemoved += Bot_ReactionRemoved;
				arg.ReactionsCleared += Bot_ReactionsCleared;
				arg.Log += Bot_Log;
				arg.UserUpdated += Bot_UserUpdated;
				arg.GuildUpdated += Bot_GuildUpdated;
				ShardsReady.Add(arg.ShardId);
			}

			HttpWebClient.SetUserAgent(arg.CurrentUser);

			Task.Run(async () =>
			{
				Thread.CurrentThread.IsBackground = true;
				await UpdateStatusAsync(arg).ConfigureAwait(false);
			});

			Log.Info($"Shard #{arg.ShardId}", "Shard Ready");

			return Task.CompletedTask;
		}

		async static Task UpdateStatusAsync(
			DiscordSocketClient client
		)
		{
			Tuple<string, ActivityType> pickedStatus = Statuses.Random();

			string replacedString = pickedStatus.Item1
				.Replace("{{PREFIX}}", SkuldApp.Configuration.Prefix)
				.Replace("{{SHARDID}}", (client.ShardId + 1).ToFormattedString())
				.Replace("{{SHARDCOUNT}}", SkuldApp.DiscordClient.Shards.Count.ToFormattedString())
				.Replace("{{TOTALSERVERCOUNT}}", SkuldApp.DiscordClient.Guilds.Count.ToFormattedString());

			await
				client
				.SetGameAsync(
					replacedString,
					type: pickedStatus.Item2
			).ConfigureAwait(false);

			await Task.Delay(SkuldRandom.Next(300, 600) * 1000).ConfigureAwait(false);
			await UpdateStatusAsync(client).ConfigureAwait(false);
		}

		private static async Task Shard_MessageDeleted(
			Cacheable<IMessage, ulong> arg1,
			ISocketMessageChannel arg2
		)
		{
			if (arg2 is IGuildChannel guildChannel)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				var gld = await Database
					.InsertOrGetGuildAsync(guildChannel.Guild)
					.ConfigureAwait(false);

				var feats = Database.Features
					.Find(guildChannel.GuildId);

				if (feats.Starboard && gld.StarDeleteIfSourceDelete)
				{
					var message = await arg1.GetOrDownloadAsync()
						.ConfigureAwait(false);

					if (Database.StarboardVotes
						.Any(x => x.MessageId == message.Id))
					{
						var vote = Database.StarboardVotes
							.FirstOrDefault(x => x.MessageId == message.Id);

						Database.StarboardVotes
							.RemoveRange(Database.StarboardVotes.ToList()
							.Where(x => x.MessageId == message.Id));

						var chan = await guildChannel.Guild
							.GetTextChannelAsync(gld.StarboardChannel)
							.ConfigureAwait(false);

						var starMessage = await chan
							.GetMessageAsync(vote.StarboardMessageId)
							.ConfigureAwait(false);

						await starMessage.DeleteAsync().ConfigureAwait(false);

						await Database.SaveChangesAsync()
							.ConfigureAwait(false);
					}
				}
			}
		}

		private static async Task Shard_MessagesBulkDeleted(
			IReadOnlyCollection<Cacheable<IMessage, ulong>> arg1,
			ISocketMessageChannel arg2
		)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			if (arg2 is IGuildChannel guildChannel)
			{
				var gld = await Database
					.InsertOrGetGuildAsync(guildChannel.Guild)
					.ConfigureAwait(false);

				var feats = Database.Features
					.Find(guildChannel.GuildId);

				if (feats.Starboard && gld.StarDeleteIfSourceDelete)
				{
					foreach (var msg in arg1)
					{
						var message = await msg.GetOrDownloadAsync()
							.ConfigureAwait(false);

						if (Database.StarboardVotes
							.Any(x => x.MessageId == message.Id))
						{
							var vote = Database.StarboardVotes
								.FirstOrDefault(x =>
									x.MessageId == message.Id
								);

							Database.StarboardVotes
								.RemoveRange(Database.StarboardVotes.ToList()
								.Where(x => x.MessageId == message.Id));

							var chan = await guildChannel.Guild
								.GetTextChannelAsync(gld.StarboardChannel)
								.ConfigureAwait(false);
							var starMessage = await chan
								.GetMessageAsync(vote.StarboardMessageId)
								.ConfigureAwait(false);

							await starMessage.DeleteAsync()
								.ConfigureAwait(false);

							await Database.SaveChangesAsync()
								.ConfigureAwait(false);
						}
					}
				}
			}
		}

		private static async Task Bot_ShardConnected(
			DiscordSocketClient arg
		)
		{
			await arg.SetGameAsync(
				$"{SkuldApp.Configuration.Prefix}help | " +
				$"{arg.ShardId + 1}/{SkuldApp.DiscordClient.Shards.Count}",
				type: ActivityType.Listening)
			.ConfigureAwait(false);

			DogStatsd.Event(
				"shards.connected",
				$"Shard {arg.ShardId} Connected",
				alertType: "info"
			);
		}

		private static Task Bot_ShardDisconnected(
			Exception arg1,
			DiscordSocketClient arg2
		)
		{
			DogStatsd.Event(
				$"Shard.disconnected",
				$"Shard {arg2.ShardId} Disconnected, error: {arg1}",
				alertType: "error"
			);
			return Task.CompletedTask;
		}

		#endregion Shards

		#region Users

		private static async Task Bot_UserJoined(
			SocketGuildUser arg
		)
		{
			DogStatsd.Increment("guild.users.joined");

			if (arg is null) return;

			//Insert into Database
			try
			{
				using SkuldDbContext database = new SkuldDbContextFactory()
					.CreateDbContext();

				await database.InsertOrGetUserAsync(arg)
					.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Error("UsrJoin", ex.Message, null, ex);
			}

			//Persistent Roles
			try
			{
				using SkuldDbContext database = new SkuldDbContextFactory()
					.CreateDbContext();

				if (database.PersistentRoles.ToList()
					.Any(x => x.UserId == arg.Id && x.GuildId == arg.Guild.Id))
				{
					foreach (var persistentRole in database.PersistentRoles
						.ToList().Where(x =>
						x.UserId == arg.Id && x.GuildId == arg.Guild.Id)
					)
					{
						try
						{
							await arg.AddRoleAsync(
								arg.Guild.GetRole(persistentRole.RoleId)
							).ConfigureAwait(false);
						}
						catch (Exception ex)
						{
							Log.Error("UsrJoin", ex.Message, null, ex);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("UsrJoin", ex.Message, null, ex);
			}

			//Join Message
			try
			{
				using SkuldDbContext database = new SkuldDbContextFactory()
					.CreateDbContext();

				var gld = await database.InsertOrGetGuildAsync(arg.Guild)
					.ConfigureAwait(false);

				if (gld is not null)
				{
					try
					{
						if (gld.JoinRole != 0)
						{
							var joinrole = arg.Guild.GetRole(gld.JoinRole);
							await arg
								.AddRoleAsync(joinrole)
							.ConfigureAwait(false);
						}
					}
					catch
					{
						Log.Error("UsrJoin", "Couldn't Give Join Role", null);
					}

					try
					{
						if (gld.JoinChannel != 0)
						{
							if (!gld.JoinImage && !string.IsNullOrEmpty(gld.JoinMessage))
							{
								var channel = arg.Guild
									.GetTextChannel(gld.JoinChannel);

								var message = gld.JoinMessage
									.ReplaceGuildEventMessage(
										arg,
										arg.Guild
									);

								await channel
									.SendMessageAsync(message)
								.ConfigureAwait(false);
							}
							else if (gld.JoinImage)
							{
								var skuldAPI = SkuldApp.Services.GetRequiredService<ISkuldAPIClient>();

								var channel = arg.Guild
									.GetTextChannel(gld.JoinChannel);

								var message = !string.IsNullOrEmpty(gld.JoinMessage) ? gld.JoinMessage
									.ReplaceGuildEventMessage(
										arg,
										arg.Guild
									) : "";

								var image = await skuldAPI.GetJoinCardAsync(arg.Id, arg.Guild.Id);

								if (image is not null)
								{
									await channel.SendFileAsync(image, "image.png", message);
								}
							}
						}
					}
					catch
					{
						Log.Error("UsrJoin", "Couldn't Send Join Message", null);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("UsrJoin", ex.Message, null, ex);
			}

			//Experience Roles
			try
			{
				using SkuldDbContext database =
					new SkuldDbContextFactory()
				.CreateDbContext();

				var feats = database.Features
					.Find(arg.Guild.Id);

				if (feats.Experience)
				{
					var rewards = database.LevelRewards
						.ToList().Where(x => x.GuildId == arg.Guild.Id);

					var usrLvl = database.UserXp
						.FirstOrDefault(x =>
							x.GuildId == arg.Guild.Id && x.UserId == arg.Id
						);

					var lvl = DatabaseUtilities.GetLevelFromTotalXP(
						usrLvl.TotalXP,
						DiscordUtilities.LevelModifier
					);

					var rolesToGive = rewards
						.Where(x => x.LevelRequired <= lvl)
						.Select(z => z.RoleId);

					if (feats.StackingRoles)
					{
						var roles = (arg.Guild as IGuild).Roles
							.Where(z => rolesToGive.Contains(z.Id));

						if (roles.Any())
						{
							try
							{
								await arg.AddRolesAsync(roles)
									.ConfigureAwait(false);
							}
							catch (Exception ex)
							{
								Log.Error("UsrJoin", ex.Message, null, ex);
							}
						}
					}
					else
					{
						var r = rewards
							.Where(x => x.LevelRequired <= lvl)
							.OrderByDescending(x => x.LevelRequired)
							.FirstOrDefault();

						if (r is not null)
						{
							var role = arg.Guild.Roles.FirstOrDefault(z =>
								rolesToGive.Contains(r.Id)
							);

							if (role is not null)
							{
								try
								{
									await arg.AddRoleAsync(role)
										.ConfigureAwait(false);
								}
								catch (Exception ex)
								{
									Log.Error("UsrJoin", ex.Message, null, ex);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("UsrJoin", ex.Message, null, ex);
			}

			Log.Verbose(Key, $"{arg} joined {arg.Guild}", null);
		}

		private static async Task Bot_UserLeft(
			SocketGuildUser arg
		)
		{
			DogStatsd.Increment("guild.users.left");

			try
			{
				using var db = new SkuldDbContextFactory().CreateDbContext();

				var gld = await db
					.InsertOrGetGuildAsync(arg.Guild)
				.ConfigureAwait(false);

				if (gld is not null)
				{
					try
					{
						if (gld.LeaveChannel != 0)
						{
							if (!gld.LeaveImage && !string.IsNullOrEmpty(gld.LeaveMessage))
							{
								var channel = arg.Guild.GetTextChannel(gld.LeaveChannel);

								var message = gld.LeaveMessage
									.ReplaceGuildEventMessage(
										arg,
										arg.Guild
									);

								await channel
									.SendMessageAsync(message)
								.ConfigureAwait(false);
							}
							else if (gld.LeaveImage)
							{
								var skuldAPI = SkuldApp.Services.GetRequiredService<ISkuldAPIClient>();

								var channel = arg.Guild.GetTextChannel(gld.LeaveChannel);

								var message = !string.IsNullOrEmpty(gld.LeaveMessage) ? gld.LeaveMessage
									.ReplaceGuildEventMessage(
										arg,
										arg.Guild
									) : "";

								var image = await skuldAPI.GetLeaveCardAsync(arg.Id, arg.Guild.Id);

								if (image is not null)
								{
									await channel.SendFileAsync(image, "image.png", message);
								}
							}
						}
					}
					catch
					{
						Log.Error("UsrLeft", "Couldn't send leave message", null);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("UsrLeft", ex.Message, null, ex);
			}

			Log.Verbose(Key, $"{arg} left {arg.Guild}", null);
		}

		private static async Task Bot_UserUpdated(
			SocketUser arg1,
			SocketUser arg2
		)
		{
			if (arg1.IsBot || arg1.IsWebhook || arg2.IsBot || arg2.IsWebhook || arg2.DiscriminatorValue == 0) return;

			try
			{
				using SkuldDbContext database = new SkuldDbContextFactory()
					.CreateDbContext();

				User suser = await database
					.InsertOrGetUserAsync(arg2)
					.ConfigureAwait(false);

				if (!suser.IsUpToDate(arg2))
				{
					suser.AvatarUrl =
						arg2.GetAvatarUrl() ?? arg2.GetDefaultAvatarUrl();

					suser.Username = arg2.Username;

					await database.SaveChangesAsync().ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				Log.Error("UsrUpdt", ex.Message, null, ex);
			}
		}

		#endregion Users

		#region Guilds

		private static async Task Bot_LeftGuild(
			SocketGuild arg
		)
		{
			Log.Verbose(Key, $"Just left {arg}", null);
			DogStatsd.Increment("guilds.left");

			try
			{
				using var database = new SkuldDbContextFactory().CreateDbContext();

				await SkuldApp.DiscordClient.SendDataAsync(
						SkuldApp.Configuration.IsDevelopmentBuild,
						SkuldApp.Configuration.DiscordGGKey,
						SkuldApp.Configuration.DBotsOrgKey,
						SkuldApp.Configuration.B4DToken
					)
				.ConfigureAwait(false);

				//MessageQueue.CheckForEmptyGuilds = true;
			}
			catch (Exception ex)
			{
				Log.Error("UsrJoin", ex.Message, null, ex);
			}
		}

		private static async Task Bot_JoinedGuild(
			SocketGuild arg
		)
		{
			DogStatsd.Increment("guilds.joined");
			Log.Verbose(Key, $"Just joined {arg}", null);

			try
			{
				using var database = new SkuldDbContextFactory().CreateDbContext();

				await SkuldApp.DiscordClient.SendDataAsync(
						SkuldApp.Configuration.IsDevelopmentBuild,
						SkuldApp.Configuration.DiscordGGKey,
						SkuldApp.Configuration.DBotsOrgKey,
						SkuldApp.Configuration.B4DToken
					)
				.ConfigureAwait(false);

				await database.InsertOrGetGuildAsync(
						arg,
						SkuldApp.Configuration.Prefix,
						SkuldApp.MessageServiceConfig.MoneyName,
						SkuldApp.MessageServiceConfig.MoneyIcon
					)
				.ConfigureAwait(false);

				//MessageQueue.CheckForEmptyGuilds = true;
			}
			catch (Exception ex)
			{
				Log.Error("UsrJoin", ex.Message, null, ex);
			}
		}

		private static async Task Bot_RoleDeleted(
			SocketRole arg
		)
		{
			DogStatsd.Increment("guilds.role.deleted");

			#region LevelRewards

			try
			{
				using var database = new SkuldDbContextFactory()
					.CreateDbContext();

				if (database.LevelRewards.Any(x => x.RoleId == arg.Id))
				{
					database.LevelRewards.RemoveRange(
						database.LevelRewards.AsQueryable()
							.Where(x => x.RoleId == arg.Id)
					);

					await database.SaveChangesAsync().ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				Log.Error("UsrJoin", ex.Message, null, ex);
			}

			#endregion LevelRewards

			#region PersistentRoles

			try
			{
				using var database = new SkuldDbContextFactory()
					.CreateDbContext();

				if (database.PersistentRoles.Any(x => x.RoleId == arg.Id))
				{
					database.PersistentRoles.RemoveRange(
						database.PersistentRoles.AsQueryable()
						.Where(x => x.RoleId == arg.Id)
					);

					await database.SaveChangesAsync().ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				Log.Error("UsrJoin", ex.Message, null, ex);
			}

			#endregion PersistentRoles

			#region IAmRoles

			try
			{
				using var database = new SkuldDbContextFactory()
					.CreateDbContext();

				if (database.IAmRoles.Any(x => x.RoleId == arg.Id))
				{
					database.IAmRoles.RemoveRange(
						database.IAmRoles.AsQueryable()
						.Where(x => x.RoleId == arg.Id)
					);

					await database.SaveChangesAsync().ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				Log.Error("UsrJoin", ex.Message, null, ex);
			}

			try
			{
				using var database = new SkuldDbContextFactory()
					.CreateDbContext();

				if (database.IAmRoles.Any(x => x.RequiredRoleId == arg.Id))
				{
					foreach (var role in database.IAmRoles.AsQueryable()
						.Where(x => x.RequiredRoleId == arg.Id))
					{
						role.RequiredRoleId = 0;
					}

					await database.SaveChangesAsync().ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				Log.Error("UsrJoin", ex.Message, null, ex);
			}

			#endregion IAmRoles

			Log.Verbose(Key, $"{arg} deleted in {arg.Guild}", null);
		}

		private static async Task Bot_GuildMemberUpdated(
			SocketGuildUser arg1,
			SocketGuildUser arg2
		)
		{
			if (arg1.IsBot || arg1.IsWebhook || arg2.IsBot || arg2.IsWebhook || arg2.DiscriminatorValue == 0) return;

			//Resync Data
			{
				using SkuldDbContext database = new SkuldDbContextFactory()
					.CreateDbContext();

				User suser = await database
					.InsertOrGetUserAsync(arg2)
					.ConfigureAwait(false);

				if (!suser.IsUpToDate(arg2))
				{
					suser.AvatarUrl =
						arg2.GetAvatarUrl() ?? arg2.GetDefaultAvatarUrl();

					suser.Username = arg2.Username;

					await database.SaveChangesAsync()
						.ConfigureAwait(false);
				}
			}

			try
			{
				if (arg1.Roles.Count != arg2.Roles.Count)
				{
					//Add Persistent Role
					await HandlePersistentRoleAdd(arg2).ConfigureAwait(false);

					//Remove Persistent Role
					await HandlePersistentRoleRemove(arg1, arg2).ConfigureAwait(false);
				}
			}
			catch
			{ }
		}

		private static async Task Bot_GuildUpdated(
			SocketGuild arg1,
			SocketGuild arg2
		)
		{
			try
			{
				using SkuldDbContext Database = new SkuldDbContextFactory()
					.CreateDbContext();

				var sguild = await
					Database.InsertOrGetGuildAsync(arg2)
				.ConfigureAwait(false);

				if (sguild.Name is null ||
					!sguild.Name.Equals(arg2.Name))
				{
					sguild.Name = arg2.Name;
				}

				if (sguild.IconUrl is null ||
					!sguild.IconUrl.Equals(arg2.IconUrl))
				{
					sguild.IconUrl = arg2.IconUrl;
				}

				await Database.SaveChangesAsync().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				Log.Error("UsrJoin", ex.Message, null, ex);
			}
		}

		private static async Task HandlePersistentRoleAdd(
			SocketGuildUser arg
		)
		{
			using SkuldDbContext database = new SkuldDbContextFactory()
				.CreateDbContext();

			var guildPersistentRoles = database.PersistentRoles
				.AsQueryable()
				.Where(x => x.GuildId == arg.Guild.Id)
				.DistinctBy(x => x.RoleId)
				.ToList();

			if (guildPersistentRoles.Count > 0)
			{
				guildPersistentRoles.ForEach(z =>
				{
					arg.Roles.ToList().ForEach(x =>
					{
						if (z.RoleId == x.Id)
						{
							if (!database.PersistentRoles
								.Any(y => y.RoleId == z.RoleId &&
								y.UserId == arg.Id &&
								y.GuildId == arg.Guild.Id)
							)
							{
								database.PersistentRoles.Add(
									new PersistentRole
									{
										GuildId = arg.Guild.Id,
										RoleId = z.RoleId,
										UserId = arg.Id
									}
								);
							}
						}
					});
				});

				await database.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		private static async Task HandlePersistentRoleRemove(
			SocketGuildUser arg1,
			SocketGuildUser arg2)
		{
			using SkuldDbContext database = new SkuldDbContextFactory()
				.CreateDbContext();

			IEnumerable<SocketRole> roleDifference;

			if (arg1.Roles.Count > arg2.Roles.Count)
			{
				roleDifference = arg1.Roles.Except(arg2.Roles);
			}
			else
			{
				roleDifference = arg2.Roles.Except(arg1.Roles);
			}

			var guildPersistentRoles = database.PersistentRoles
				.AsQueryable()
				.Where(x => x.GuildId == arg2.Guild.Id)
				.DistinctBy(x => x.RoleId)
				.ToList();

			var roles = new List<ulong>();

			guildPersistentRoles.ForEach(z =>
			{
				if (roleDifference.Any(x => x.Id == z.RoleId))
				{
					roles.Add(z.RoleId);
				}
			});

			if (roles.Count > 0)
			{
				database.PersistentRoles.RemoveRange(
					database.PersistentRoles.ToList()
					.Where(x =>
						roles.Contains(x.RoleId) &&
						x.UserId == arg2.Id &&
						x.GuildId == arg2.Guild.Id
					)
				);

				await database.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		#endregion Guilds
	}
}