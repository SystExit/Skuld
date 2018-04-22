using Discord.WebSocket;
using System.Threading.Tasks;
using StatsdClient;
using System;
using Skuld.Utilities;
using Discord;
using Skuld.Models;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Skuld.Services
{
    public class MessageService
    {
		DiscordShardedClient client;
		DatabaseService database;
		LoggingService logger;
		public MessageServiceConfig config;
		public CommandService commandService;

		public MessageService(DiscordShardedClient cli,
			DatabaseService db,
			MessageServiceConfig serviceConfig,
			LoggingService log) //inherits from depinjection
		{
			client = cli;
			database = db;
			config = serviceConfig;
			logger = log;
		}

		public async Task ConfigureAsync(CommandServiceConfig config, IServiceProvider services)
		{
			commandService = new CommandService(config);
			var logger = services.GetRequiredService<LoggingService>();
			commandService.Log += logger.DiscordLogger;
			await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), services);
		}

		public async Task OnMessageRecievedAsync(SocketMessage arg)
		{
			DogStatsd.Increment("messages.recieved");

			if (arg.Author.IsBot) return;

			var message = arg as SocketUserMessage;
			if (message == null) return;

			var context = new ShardedCommandContext(client, message);

			SkuldGuild sguild = null;
			if (context.Guild != null)
				sguild = await database.GetGuildAsync(context.Guild.Id);

			if (HasPrefix(message,sguild))
			{
				var customcommand = await MessageTools.GetCustomCommandAsync(context.Guild, MessageTools.GetCommandName("", 0, arg), database);
				if (customcommand != null)
				{
					//Do stuff
					await DispatchCommandAsync(context, customcommand);
					return;
				}

				if (MessageTools.IsEnabledChannel((ITextChannel)context.Channel))
				{
					if (MessageTools.CheckForPrefixReset(context, client))
					{
						sguild.Prefix = config.Prefix;
						await database.UpdateGuildAsync(sguild);
						await SendChannelAsync(context.Channel, "Reset the prefix back to `sk!`");
						return;
					}

					var cmds = commandService.Search(context, GetCmdName(message, sguild)).Commands;

					if(cmds!=null) await DispatchCommandAsync(context, cmds.FirstOrDefault().Command);
				}
			}
		}

		string GetCmdName(SocketUserMessage arg, SkuldGuild sguild = null)
		{
			string content = "";
			var contentsplit = arg.Content.Split(' ')[0];
			if (config.AltPrefix != null)
			{
				if (database.CanConnect)
				{
					if (contentsplit.StartsWith(sguild.Prefix))
						content = contentsplit.Replace(sguild.Prefix, "");
					if (contentsplit.StartsWith(config.Prefix))
						content = contentsplit.Replace(config.Prefix, "");
					if (contentsplit.StartsWith(config.AltPrefix))
						content = contentsplit.Replace(config.AltPrefix, "");
				}
				else
				{
					if (contentsplit.StartsWith(config.Prefix))
						content = contentsplit.Replace(config.Prefix, "");
					if (contentsplit.StartsWith(config.AltPrefix))
						content = contentsplit.Replace(config.AltPrefix, "");
				}
			}
			else
			{
				if (database.CanConnect)
				{
					if (contentsplit.StartsWith(sguild.Prefix))
						content = contentsplit.Replace(sguild.Prefix, "");
					if (contentsplit.StartsWith(config.Prefix))
						content = contentsplit.Replace(config.Prefix, "");
				}
				else
				{
					if (contentsplit.StartsWith(config.Prefix))
						content = contentsplit.Replace(config.Prefix, "");
				}
			}
			return content;
		}

		bool HasPrefix(SocketUserMessage message, SkuldGuild guild = null)
		{
			bool retn;
			if (guild!=null)
			{
				retn = message.HasStringPrefix(guild.Prefix, ref config.ArgPos) | message.HasStringPrefix(config.Prefix, ref config.ArgPos) |
					message.HasStringPrefix(config.AltPrefix, ref config.ArgPos);
			}
			else
			{
				retn = message.HasStringPrefix(config.Prefix, ref config.ArgPos) |
					message.HasStringPrefix(config.AltPrefix, ref config.ArgPos);
			}
			return retn;
		}

		public async Task DispatchCommandAsync(ShardedCommandContext context, CustomCommand command)
		{
			await context.Channel.TriggerTypingAsync();
			await SendChannelAsync(context.Channel, command.Content);
			await InsertCommand(command, context.Message);
			DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:custom", $"cmd:{command.CommandName.ToLowerInvariant()}" });
		}
		public async Task DispatchCommandAsync(ShardedCommandContext context, CommandInfo command)
		{
			await context.Channel.TriggerTypingAsync();
			var watch = new Stopwatch();
			watch.Start();
			var result = await commandService.ExecuteAsync(context, config.ArgPos, Bot.services);
			watch.Stop();
			if (result.IsSuccess)
			{
				DogStatsd.Histogram("commands.latency", watch.ElapsedMilliseconds(), 0.5, new[] { $"module:{command.Module.Name.ToLowerInvariant()}", $"cmd:{command.Name.ToLowerInvariant()}" });
				await InsertCommand(command, context.Message);
				DogStatsd.Increment("commands.processed", 1, 1, new[] { $"module:{command.Module.Name.ToLowerInvariant()}", $"cmd:{command.Name.ToLowerInvariant()}" });
			}

			if (!result.IsSuccess)
			{
				bool displayerror = true;
				if (result.ErrorReason.Contains("few parameters"))
				{
					var cmdembed = GetCommandHelp(context, command.Name);
					await SendChannelAsync(context.Channel, "You seem to be missing a parameter or 2, here's the help", cmdembed);
					displayerror = false;
				}

				if (result.Error != CommandError.UnknownCommand && !result.ErrorReason.Contains("Timeout") && displayerror)
				{
					await logger.AddToLogsAsync(new Models.LogMessage("CmdHand", "Error with command, Error is: " + result, LogSeverity.Error));
					await SendChannelAsync(context.Channel, "", new EmbedBuilder { Author = new EmbedAuthorBuilder { Name = "Error with the command" }, Description = Convert.ToString(result.ErrorReason), Color = new Color(255, 0, 0) }.Build());
				}
				DogStatsd.Increment("commands.errors");

				switch (result.Error)
				{
					case CommandError.UnmetPrecondition:
						DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:unm-precon" });
						break;
					case CommandError.Unsuccessful:
						DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:generic" });
						break;
					case CommandError.MultipleMatches:
						DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:multiple" });
						break;
					case CommandError.BadArgCount:
						DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:incorr-args" });
						break;
					case CommandError.ParseFailed:
						DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:parse-fail" });
						break;
					case CommandError.Exception:
						DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:exception" });
						break;
					case CommandError.UnknownCommand:
						DogStatsd.Increment("commands.errors", 1, 1, new[] { "err:unk-cmd" });
						break;
				}
				return;
			}
		}
		
		
		//MessageSending
		public async Task<IUserMessage> SendChannelAsync(IChannel channel, string message)
		{
			try
			{
				var textChan = (ITextChannel)channel;
				var mesgChan = (IMessageChannel)channel;
				await mesgChan.TriggerTypingAsync();
				await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
				return await mesgChan.SendMessageAsync(message);
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
				return null;
			}
		}
		public async Task<IUserMessage> SendChannelAsync(IChannel channel, string message, Embed embed)
		{
			try
			{
				var textChan = (ITextChannel)channel;
				var mesgChan = (IMessageChannel)channel;
				IUserMessage msg;
				await mesgChan.TriggerTypingAsync();
				var perm = (await textChan.Guild.GetUserAsync(client.CurrentUser.Id)).GetPermissions(textChan).EmbedLinks;
				if (!perm)
				{
					if (message != null)
					{
						msg = await mesgChan.SendMessageAsync(message + "\n" + EmbedToText.ConvertEmbedToText(embed));
					}
					else
					{
						msg = await mesgChan.SendMessageAsync(EmbedToText.ConvertEmbedToText(embed));
					}
				}
				else
				{
					msg = await mesgChan.SendMessageAsync(message, false, embed);
				}
				await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
				return msg;
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
				return null;
			}
		}
		public async Task<IUserMessage> SendChannelAsync(IChannel channel, string message, string filename)
		{
			var textChan = (ITextChannel)channel;
			var mesgChan = (IMessageChannel)channel;
			IUserMessage msg;
			await mesgChan.TriggerTypingAsync();
			try
			{
				msg = await mesgChan.SendFileAsync(filename, message);
				await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
				return msg;
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
				return null;
			}
		}

		public async Task SendChannelAsync(IChannel channel, string message, double timeout)
		{
			try
			{
				var textChan = (ITextChannel)channel;
				var mesgChan = (IMessageChannel)channel;
				await mesgChan.TriggerTypingAsync();
				IUserMessage msg = await mesgChan.SendMessageAsync(message);
				await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
				await Task.Delay((int)(timeout * 1000));
				DeleteMessage(msg);
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
			}
		}
		public async Task SendChannelAsync(IChannel channel, string message, double timeout, Embed embed)
		{
			try
			{
				var textChan = (ITextChannel)channel;
				var mesgChan = (IMessageChannel)channel;
				await mesgChan.TriggerTypingAsync();
				var perm = (await textChan.Guild.GetUserAsync(client.CurrentUser.Id)).GetPermissions(textChan).EmbedLinks;
				IUserMessage msg = null;
				if (!perm)
				{
					if (message != null)
					{
						msg = await mesgChan.SendMessageAsync(message + "\n" + EmbedToText.ConvertEmbedToText(embed));
					}
					else
					{
						msg = await mesgChan.SendMessageAsync(EmbedToText.ConvertEmbedToText(embed));
					}
				}
				else
				{
					msg = await mesgChan.SendMessageAsync(message, isTTS: false, embed: embed);
				}
				await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
				await Task.Delay((int)(timeout * 1000));
				DeleteMessage(msg);
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
			}
		}
		public async Task SendChannelAsync(IChannel channel, string message, double timeout, string filename)
		{
			var textChan = (ITextChannel)channel;
			var mesgChan = (IMessageChannel)channel;
			await mesgChan.TriggerTypingAsync();
			try
			{
				IUserMessage msg = await mesgChan.SendFileAsync(filename, message);
				await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Dispatched message to {(channel as IGuildChannel).Guild} in {(channel as IGuildChannel).Name}", LogSeverity.Info));
				await Task.Delay((int)(timeout * 1000));
				DeleteMessage(msg);
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("MH-ChNV", "Error dispatching Message, printed exception to logs.", LogSeverity.Warning, ex));
			}
		}

		private async void DeleteMessage(IUserMessage msg)
		{
			if (msg != null)
			{
				await msg.DeleteAsync();
				await logger.AddToLogsAsync(new Models.LogMessage("MsgDisp", $"Deleted a timed message", LogSeverity.Info));
			}
		}

		public async Task<IUserMessage> SendDMsAsync(IMessageChannel channel, IDMChannel user, string message)
		{
			try
			{
				IUserMessage msg = null;
				msg = await user.SendMessageAsync(message);
				await SendChannelAsync(channel, $":ok_hand: <@{user.Recipient.Id}> Check your DMs.");
				return msg;
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("MsgH-DM", "Error dispatching Direct Message to user, sending to channel instead. Printed exception to logs.", LogSeverity.Warning, ex));
				await SendChannelAsync(channel, "I couldn't send it to your DMs, so I sent it here instead... I hope you're not mad. <:blobcry:350681079415439361> " + message);
				return null;
			}
		}
		public async Task<IUserMessage> SendDMsAsync(IMessageChannel channel, IDMChannel user, string message, Embed embed)
		{
			try
			{
				IUserMessage msg = null;
				msg = await user.SendMessageAsync(message, isTTS: false, embed: embed);
				await SendChannelAsync(channel, $":ok_hand: <@{user.Recipient.Id}> Check your DMs.");
				return msg;
			}
			catch (Exception ex)
			{
				await logger.AddToLogsAsync(new Models.LogMessage("MsgH-DM", "Error dispatching Direct Message to user, sending to channel instead. Printed exception to logs.", LogSeverity.Warning, ex));
				await SendChannelAsync(channel, "I couldn't send it to your DMs, so I sent it here instead... I hope you're not mad. <:blobcry:350681079415439361> " + message, embed);
				return null;
			}
		}

		//tools
		Embed GetCommandHelp(ICommandContext context, string command)
		{
			if (command.ToLower() != "pasta")
			{
				var result = commandService.Search(context, command);

				if (!result.IsSuccess)
				{
					return null;
				}

				var embed = new EmbedBuilder()
				{
					Description = $"Here are some commands like **{command}**",
					Color = Tools.Tools.RandomColor()
				};

				var cmd = result.Commands.FirstOrDefault();

				var summ = GetSummaryAsync(cmd.Command, result.Commands, command);

				embed.AddField(x =>
				{
					x.Name = string.Join(", ", cmd.Command.Aliases);
					x.Value = summ;
					x.IsInline = false;
				});

				return embed.Build();
			}
			return null;
		}

		string GetSummaryAsync(CommandInfo cmd, IReadOnlyList<CommandMatch> Commands, string comm)
		{
			string summ = "Summary: " + cmd.Summary;
			int totalparams = 0;
			foreach (var com in Commands)
				totalparams += com.Command.Parameters.Count;

			if (totalparams > 0)
			{
				summ += "\nParameters:\n";

				foreach (var param in cmd.Parameters)
				{
					if (param.IsOptional)
					{
						summ += $"**[Optional]** {param.Name} - {param.Type.Name}\n";
					}
					else
					{
						summ += $"**[Required]** {param.Name} - {param.Type.Name}\n";
					}
				}

				return summ;
			}
			return summ + "\nParameters: None";
		}

		private async Task InsertCommand(CommandInfo command, SocketMessage arg)
		{
			var user = arg.Author;
			var cmd = new MySqlCommand("SELECT UserUsage FROM commandusage WHERE UserID = @userid AND Command = @command");
			cmd.Parameters.AddWithValue("@userid", user.Id);
			cmd.Parameters.AddWithValue("@command", command.Name ?? command.Module.Name);
			var resp = await database.GetSingleAsync(cmd);
			if (!String.IsNullOrEmpty(resp))
			{
				var cmdusg = Convert.ToInt32(resp);
				cmdusg = cmdusg + 1;
				cmd = new MySqlCommand("UPDATE commandusage SET UserUsage = @userusg WHERE UserID = @userid AND Command = @command");
				cmd.Parameters.AddWithValue("@userusg", cmdusg);
				cmd.Parameters.AddWithValue("@userid", user.Id);
				cmd.Parameters.AddWithValue("@command", command.Name ?? command.Module.Name);
				await database.NonQueryAsync(cmd);
			}
			else
			{
				cmd = new MySqlCommand("INSERT INTO commandusage (`UserID`, `UserUsage`, `Command`) VALUES (@userid , @userusg , @command)");
				cmd.Parameters.AddWithValue("@userusg", 1);
				cmd.Parameters.AddWithValue("@userid", user.Id);
				cmd.Parameters.AddWithValue("@command", command.Name ?? command.Module.Name);
				await database.NonQueryAsync(cmd);
			}
		}
		private async Task InsertCommand(CustomCommand command, SocketMessage arg)
		{
			var user = arg.Author;
			var cmd = new MySqlCommand("SELECT UserUsage FROM commandusage WHERE UserID = @userid AND Command = @command");
			cmd.Parameters.AddWithValue("@userid", user.Id);
			cmd.Parameters.AddWithValue("@command", command.CommandName);
			var resp = await database.GetSingleAsync(cmd);
			if (!String.IsNullOrEmpty(resp))
			{
				var cmdusg = Convert.ToInt32(resp);
				cmdusg = cmdusg + 1;
				cmd = new MySqlCommand("UPDATE commandusage SET UserUsage = @userusg WHERE UserID = @userid AND Command = @command");
				cmd.Parameters.AddWithValue("@userusg", cmdusg);
				cmd.Parameters.AddWithValue("@userid", user.Id);
				cmd.Parameters.AddWithValue("@command", command.CommandName);
				await database.NonQueryAsync(cmd);
			}
			else
			{
				cmd = new MySqlCommand("INSERT INTO commandusage (`UserID`, `UserUsage`, `Command`) VALUES (@userid , @userusg , @command)");
				cmd.Parameters.AddWithValue("@userusg", 1);
				cmd.Parameters.AddWithValue("@userid", user.Id);
				cmd.Parameters.AddWithValue("@command", command.CommandName);
				await database.NonQueryAsync(cmd);
			}
		}
	}
}
