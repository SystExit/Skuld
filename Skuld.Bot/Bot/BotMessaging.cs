using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Skuld.Bot.Extensions;
using Skuld.Core.Extensions;
using Skuld.Core.Extensions.Verification;
using Skuld.Core.Utilities;
using Skuld.Models;
using Skuld.Services.Accounts.Experience;
using Skuld.Services.CustomCommands;
using Skuld.Services.Messaging.Extensions;
using StatsdClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Skuld.Bot
{
	internal static class BotMessaging
	{
		private const string Key = "MsgHand";
		private static Stopwatch watch;

		internal static void Configure()
		{
			watch = new Stopwatch();
		}

		#region CommandService Logs

		internal static Task CommandService_Log(LogMessage arg)
		{
			var key = $"{Key}-{arg.Source}";
			switch (arg.Severity)
			{
				case LogSeverity.Error:
					Log.Error(key, arg.Message, null, arg.Exception);
					break;

				case LogSeverity.Debug:
					Log.Debug(key, arg.Message, null, arg.Exception);
					break;

				case LogSeverity.Critical:
					Log.Critical(key, arg.Message, null, arg.Exception);
					break;

				case LogSeverity.Info:
					Log.Info(key, arg.Message);
					break;

				case LogSeverity.Verbose:
					Log.Verbose(key, arg.Message, null, arg.Exception);
					break;

				case LogSeverity.Warning:
					Log.Warning(key, arg.Message, null, arg.Exception);
					break;
			}
			return Task.CompletedTask;
		}

		internal static async Task CommandService_CommandExecuted(
			Optional<CommandInfo> arg1,
			ICommandContext arg2,
			IResult arg3
		)
		{
			CommandInfo cmd = null;
			string name = "";

			if (arg1.IsSpecified)
			{
				cmd = arg1.Value;

				name = cmd.Module.GetModulePath();

				if (cmd.Name != null)
				{
					name += "." + cmd.Name;
				}

				name = name
					.ToLowerInvariant()
					.Replace(" ", "-")
					.Replace("/", ".");
			}

			if (arg3.IsSuccess)
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				if (arg1.IsSpecified)
				{
					var cont = arg2 as ShardedCommandContext;

					DogStatsd.Increment("commands.total.threads", 1, 1, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{name}" });

					DogStatsd.Histogram("commands.latency", watch.ElapsedMilliseconds, 0.5, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{name}" });

					var usr = await Database.InsertOrGetUserAsync(cont.User).ConfigureAwait(false);

					await InsertCommandAsync(cmd, usr).ConfigureAwait(false);

					DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{name}" });
				}
			}
			else
			{
				bool displayerror = true;
				if (arg3.ErrorReason.Contains("few parameters"))
				{
					var prefix = MessageTools.GetPrefixFromCommand(arg2.Message.Content, SkuldApp.Configuration.Prefix, SkuldApp.Configuration.AltPrefix);

					string cmdName = "";

					if (arg2.Guild != null)
					{
						using var Database = new SkuldDbContextFactory().CreateDbContext();

						prefix = MessageTools.GetPrefixFromCommand(arg2.Message.Content,
							SkuldApp.Configuration.Prefix,
							SkuldApp.Configuration.AltPrefix,
							(await Database.InsertOrGetGuildAsync(arg2.Guild).ConfigureAwait(false)).Prefix
						);
					}

					if (cmd != null && cmd.Module.Group != null)
					{
						string pfx = "";

						ModuleInfo mod = cmd.Module;
						while (mod.Group != null)
						{
							pfx += $"{mod.Group} ";

							if (mod.IsSubmodule)
							{
								mod = cmd.Module.Parent;
							}
						}

						cmdName = $"{pfx}{cmd.Name}";

						var cmdembed = await SkuldApp.CommandService.GetCommandHelpAsync(arg2, cmdName, prefix).ConfigureAwait(false);
						await
							arg2.Channel.SendMessageAsync(
								"You seem to be missing a parameter or 2, here's the help",
								embed: cmdembed.Build()
							)
						.ConfigureAwait(false);
						displayerror = false;
					}
				}

				if (arg3.ErrorReason.Contains("Timeout"))
				{
					var hourglass = new Emoji("⏳");
					if (!arg2.Message.Reactions.Any(x => x.Key == hourglass && x.Value.IsMe))
					{
						await arg2.Message.AddReactionAsync(hourglass).ConfigureAwait(false);
					}
					displayerror = false;
				}

				if (arg3.Error != CommandError.UnknownCommand && displayerror)
				{
					Log.Error(Key,
						"Error with command, Error is: " + arg3,
						arg2 as ShardedCommandContext);

					await EmbedExtensions.FromError(arg3.ErrorReason, arg2)
						.QueueMessageAsync(arg2)
					.ConfigureAwait(false);
				}

				switch (arg3.Error)
				{
					case CommandError.UnmetPrecondition:
						DogStatsd.Increment("commands.errors",
							1,
							1,
							new[] {
								"err:unm-precon",
								$"mod:{cmd.Module.Name}",
								$"cmd:{name}"
							}
						);
						break;

					case CommandError.Unsuccessful:
						DogStatsd.Increment("commands.errors",
							1,
							1,
							new[] {
								"err:generic",
								$"mod:{cmd.Module.Name}",
								$"cmd:{name}"
							}
						);
						break;

					case CommandError.MultipleMatches:
						DogStatsd.Increment("commands.errors",
							1,
							1,
							new[] {
								"err:multiple",
								$"mod:{cmd.Module.Name}",
								$"cmd:{name}"
							}
						);
						break;

					case CommandError.BadArgCount:
						DogStatsd.Increment("commands.errors",
							1,
							1,
							new[] {
								"err:incorr-args",
								$"mod:{cmd.Module.Name}",
								$"cmd:{name}"
							}
						);
						break;

					case CommandError.ParseFailed:
						DogStatsd.Increment("commands.errors",
							1,
							1,
							new[] {
								"err:parse-fail",
								$"mod:{cmd.Module.Name}",
								$"cmd:{name}"
							}
						);
						break;

					case CommandError.Exception:
						DogStatsd.Increment("commands.errors",
							1,
							1,
							new[] {
								"err:exception",
								$"mod:{cmd.Module.Name}",
								$"cmd:{name}"
							}
						);
						break;

					case CommandError.UnknownCommand:
						DogStatsd.Increment("commands.errors",
							1,
							1,
							new[] {
								"err:unk-cmd"
							}
						);
						break;
				}
			}
			watch = new Stopwatch();

			try
			{
				var message = arg2.Message as SocketUserMessage;

				using var Database = new SkuldDbContextFactory().CreateDbContext();

				User suser = await Database.InsertOrGetUserAsync(message.Author).ConfigureAwait(false);

				{
					var keys = Database.DonatorKeys.AsAsyncEnumerable().Where(x => x.Redeemer == suser.Id);

					if (await keys.AnyAsync().ConfigureAwait(false))
					{
						bool hasChanged = false;
						var current = DateTime.Now;
						await keys.ForEachAsync(x =>
						{
							if (current > x.RedeemedWhen.FromEpoch().AddDays(365))
							{
								Database.DonatorKeys.Remove(x);
								hasChanged = true;
							}
						}).ConfigureAwait(false);

						if (hasChanged)
						{
							if (!await Database.DonatorKeys.AsAsyncEnumerable().Where(x => x.Redeemer == suser.Id).AnyAsync().ConfigureAwait(false))
							{
								suser.Flags -= DiscordUtilities.BotDonator;
							}
							await Database.SaveChangesAsync().ConfigureAwait(false);
						}
					}
				}

				if (!suser.IsUpToDate(message.Author as SocketUser))
				{
					suser.AvatarUrl = message.Author.GetAvatarUrl() ?? message.Author.GetDefaultAvatarUrl();
					suser.Username = message.Author.Username;
					await Database.SaveChangesAsync().ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				Log.Error(Key, ex.Message, arg2, ex);
			}
		}

		#endregion CommandService Logs

		#region HandleProcessing

		public static Task HandleMessage(SocketMessage arg)
		{
			Task.Run(() => HandleMessageAsync(arg));

			return Task.CompletedTask;
		}

		private static async Task HandleMessageAsync(SocketMessage arg)
		{
			DogStatsd.Increment("messages.recieved");

			if (arg.Author.IsBot ||
				arg.Author.IsWebhook ||
				arg.Author.DiscriminatorValue == 0 ||
				arg is not SocketUserMessage message) return;

			ShardedCommandContext context = new ShardedCommandContext(
				SkuldApp.DiscordClient,
				message
			);

			Guild sguild = null;

			if (context.Guild != null)
			{
				if (!await CheckPermissionToSendMessageAsync(context.Channel as ITextChannel).ConfigureAwait(false)) return;

				if (SkuldApp.DiscordClient.GetShardFor(context.Guild).ConnectionState != ConnectionState.Connected) return;

				_ = Task.Run(() => HandleSideTasksAsync(context));

				var guser = await
					(context.Guild as IGuild).GetCurrentUserAsync()
				.ConfigureAwait(false);

				var guildMem = await
					(context.Guild as IGuild).GetUserAsync(message.Author.Id)
				.ConfigureAwait(false);

				if (!guser.GetPermissions(context.Channel as IGuildChannel).SendMessages) return;

				if (!MessageTools.IsEnabledChannel(guildMem, context.Channel as ITextChannel)) return;

				using var Database = new SkuldDbContextFactory().CreateDbContext();

				sguild = await
				   Database.InsertOrGetGuildAsync(context.Guild)
				.ConfigureAwait(false);

				if (sguild.Name != context.Guild.Name)
				{
					sguild.Name = context.Guild.Name;

					await Database.SaveChangesAsync().ConfigureAwait(false);
				}

				if (sguild.IconUrl != context.Guild.IconUrl)
				{
					sguild.IconUrl = context.Guild.IconUrl;

					await Database.SaveChangesAsync().ConfigureAwait(false);
				}
			}

			if (!MessageTools.HasPrefix(message, SkuldApp.Configuration.Prefix, SkuldApp.Configuration.AltPrefix, sguild?.Prefix)) return;

			try
			{
				using var Database = new SkuldDbContextFactory().CreateDbContext();

				User suser = await Database.InsertOrGetUserAsync(message.Author).ConfigureAwait(false);

				if (suser != null &&
					suser.Flags.IsBitSet(DiscordUtilities.Banned) &&
					(!suser.Flags.IsBitSet(DiscordUtilities.BotCreator) ||
					!suser.Flags.IsBitSet(DiscordUtilities.BotAdmin))
				) return;

				var prefix = MessageTools.GetPrefixFromCommand(context.Message.Content, SkuldApp.Configuration.Prefix, SkuldApp.Configuration.AltPrefix, sguild?.Prefix);

				if (prefix != null)
				{
					await DispatchCommandAsync(context, prefix).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(Key, ex.Message, context, ex);
			}
		}

		public static async Task HandleSideTasksAsync(ICommandContext context)
		{
			using var Database = new SkuldDbContextFactory().CreateDbContext();

			Guild sguild = null;

			if (context.Guild != null)
			{
				sguild = await
				   Database.InsertOrGetGuildAsync(context.Guild)
				.ConfigureAwait(false);
			}

			if (sguild != null)
			{
				if (!Database.Features.Any(x => x.Id == sguild.Id))
				{
					Database.Features.Add(new GuildFeatures
					{
						Id = sguild.Id
					});
					await Database.SaveChangesAsync().ConfigureAwait(false);
				}

				GuildFeatures features = Database.Features.Find(sguild.Id);
				if (features.Experience)
				{
					_ = Task.Run(() => ExperienceService.HandleExperienceAsync(context));
				}

				if (!Database.Modules.Any(x => x.Id == sguild.Id))
				{
					Database.Modules.Add(new GuildModules
					{
						Id = sguild.Id
					});
					await Database.SaveChangesAsync().ConfigureAwait(false);
				}

				GuildModules modules;
				if (!Database.Modules.Any(x => x.Id == sguild.Id))
				{
					Database.Modules.Add(new GuildModules
					{
						Id = sguild.Id
					});
					await Database.SaveChangesAsync().ConfigureAwait(false);
				}

				modules = Database.Modules.Find(sguild.Id);

				if (modules.Custom)
				{
					_ = Task.Run(() => CustomCommandService.HandleCustomCommandAsync(context, SkuldApp.Configuration));
				}
			}
		}

		#endregion HandleProcessing

		#region Dispatching

		public static async Task DispatchCommandAsync(ICommandContext context, string prefix)
		{
			try
			{
				watch.Start();
				await SkuldApp.CommandService.ExecuteAsync(context, prefix.Length, SkuldApp.Services, MultiMatchHandling.Exception).ConfigureAwait(false);
				watch.Stop();
			}
			catch (Exception ex)
			{
				Log.Critical("CmdService", ex.Message, context, ex);
			}
		}

		#endregion Dispatching

		#region HandleInsertion

		private static async Task InsertCommandAsync(CommandInfo command, User user)
		{
			var name = command.Name ?? command.Module.Name;

			if (string.IsNullOrEmpty(name))
			{
				if (command.Module.IsSubmodule)
				{
					ModuleInfo parentModule = command.Module.Parent;
					while (string.IsNullOrEmpty(name))
					{
						name = parentModule.Name;

						if (string.IsNullOrEmpty(name) && parentModule.IsSubmodule)
						{
							parentModule = parentModule.Parent;
						}
						else if (string.IsNullOrEmpty(name) && !parentModule.IsSubmodule)
						{
							break;
						}
					}
				}
			}

			if (string.IsNullOrEmpty(name))
			{
				return;
			}

			using var Database = new SkuldDbContextFactory().CreateDbContext();

			var experience = Database.UserCommandUsage.FirstOrDefault(x => x.UserId == user.Id && x.Command.ToLower() == name.ToLower());
			if (experience != null)
			{
				experience.Usage += 1;
			}
			else
			{
				Database.UserCommandUsage.Add(new UserCommandUsage
				{
					Command = name,
					UserId = user.Id,
					Usage = 1
				});
			}

			await Database.SaveChangesAsync().ConfigureAwait(false);
		}

		#endregion HandleInsertion

		public static async Task<bool> CheckPermissionToSendMessageAsync(ITextChannel channel)
		{
			if (channel.Guild != null)
			{
				var currentuser = await channel.Guild.GetCurrentUserAsync().ConfigureAwait(false);
				var chan = await channel.Guild.GetChannelAsync(channel.Id).ConfigureAwait(false);
				var po = chan.GetPermissionOverwrite(currentuser);

				if (po.HasValue)
				{
					if (po.Value.SendMessages != PermValue.Deny)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}