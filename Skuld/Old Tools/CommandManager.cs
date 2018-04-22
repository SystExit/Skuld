using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Linq;
using MySql.Data.MySqlClient;
using StatsdClient;
using Skuld.Models;
using Skuld.Utilities;

namespace Skuld.Tools
{
    public class OldCommandManager : Bot
    {
        private static string defaultPrefix = Configuration.Prefix;
        private static string customPrefix;
        public static async Task Bot_MessageReceived(SocketMessage arg)
        {
			try
			{
				DogStatsd.Increment("messages.recieved");

				var message = arg as SocketUserMessage;
				if (arg.Author.IsBot) return;
				if (message == null) return;

				if(message.Channel.Id != arg.Author.Id)
				{
					var textchannel = (ITextChannel)message.Channel;

					if (textchannel != null && textchannel.Topic != null && textchannel.Topic.Contains("-commands")) return;
				}

				int argPos = 0;
				var chan = message.Channel as SocketGuildChannel;
				var context = new ShardedCommandContext(bot, message);

				if (!String.IsNullOrEmpty(Configuration.SqlDBHost) && Database.CanConnect)
				{
					await CheckUser(context.User);
					customPrefix = await Database.GetSingleAsync("guild", "prefix", "ID", context.Guild.Id.ToString());

					if (message.Content.ToLower() == $"{bot.CurrentUser.Username.ToLower()}.resetprefix")
					{
						var guilduser = message.Author as SocketGuildUser;
						await Logger.AddToLogsAsync(new Models.LogMessage("HandCmd", $"Prefix reset on guild {context.Guild.Name}", LogSeverity.Info));
						if (guilduser.GuildPermissions.ManageGuild)
						{
							var command = new MySqlCommand("UPDATE guild SET prefix = @prefix WHERE ID = @guildid");
							command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
							command.Parameters.AddWithValue("@prefix", defaultPrefix);
							await Database.NonQueryAsync(command).ContinueWith(async x =>
							{
								string newprefix = await Database.GetSingleAsync("guild", "prefix", "ID", context.Guild.Id.ToString());
								if (newprefix == defaultPrefix)
								{
									await MessageHandler.SendChannelAsync(message.Channel as SocketTextChannel, $"Successfully reset the prefix, it is now `{newprefix}`");
								}
								DogStatsd.Increment("commands.processed");
							});
						}
						else
						{
							DogStatsd.Increment("commands.errors",1,1, new string[]{ "unm-precon" });
							await MessageHandler.SendChannelAsync(message.Channel, "I'm sorry, you don't have permissions to reset the prefix, you need `MANAGE_SERVER` or `ADMINISTRATOR`");
						}
					}
				}

				if (String.IsNullOrEmpty(customPrefix))
				{
					customPrefix = defaultPrefix;
				}

				await DispatchCommand(message, context, argPos, arg);
			}
			catch (Exception ex)
			{
				await Logger.AddToLogsAsync(new Models.LogMessage("MsgRcvd", "Error processing command", LogSeverity.Error, ex));

				await MessageHandler.SendChannelAsync(arg.Channel, "Something happened. :(");
			}
        }     

        private static async Task CheckUser(SocketUser usr)
        {
			try
			{
				if (await Database.GetUserAsync(usr.Id) == null)
				{
					var res = await Database.InsertUserAsync(usr);
					if (res.Successful)
					{
						await Logger.AddToLogsAsync(new Models.LogMessage("ChkUsr", $"Added {usr.Username} to database", LogSeverity.Info));
					}
				}
			}
			catch (Exception ex)
			{
				await Logger.AddToLogsAsync(new Models.LogMessage("Cmd-Mgr", "Couldn't fix user from being missing", LogSeverity.Error, ex));
			}			
        }
        private static async Task CheckGuild(SocketGuild gld)
        {
            try
            {
                if (await Database.GetGuildAsync(gld.Id) == null)
                    await Events.DiscordEvents.PopulateSpecificGuild(gld);
            }
            catch (Exception ex)
            {
                await Logger.AddToLogsAsync(new Models.LogMessage("Cmd-Mgr", "Couldn't fix guild not existing.", LogSeverity.Error, ex));
            }
        }

        private static async Task DispatchCommand(SocketUserMessage message, ShardedCommandContext context, int argPos, SocketMessage arg)
        {
            try
            {
				if (HasPrefix(message, argPos))
				{
					IResult result = null;
					CommandInfo cmd = null;
					var command = commands.Search(context, arg.Content.Split(' ')[0].Replace(defaultPrefix, "").Replace(customPrefix, "")).Commands;
					if (command != null) cmd = command.FirstOrDefault().Command;
					if (Database.CanConnect)
					{
						var guild = await Database.GetGuildAsync(context.Guild.Id);
						if (guild != null)
						{

								//cmd = cmd = command.FirstOrDefault().Command;
								//var res = command.FirstOrDefault();
								//var module = CheckModule(guild, res.Command.Module);
								//if (module)
								//{
									var watch = new Stopwatch();
									watch.Start();
									result = await commands.ExecuteAsync(context, argPos, services);
									watch.Stop();
									//if (result.IsSuccess)
									//{ await InsertCommand(cmd, message); DogStatsd.Histogram("commands.latency", watch.ElapsedMilliseconds(), 0.5, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{cmd.Name.ToLowerInvariant()}" }); }
								//}
							
						}
					}
					else
					{
						//cmd = command.FirstOrDefault().Command;
						var watch = new Stopwatch();
						watch.Start();
						result = await commands.ExecuteAsync(context, argPos, services);
						watch.Stop();
						//if (result.IsSuccess)
						//{ DogStatsd.Histogram("commands.latency", watch.ElapsedMilliseconds(), 0.5, new[] { $"module:{cmd.Module.Name.ToLowerInvariant()}", $"cmd:{cmd.Name.ToLowerInvariant()}" }); }
					}
					if (result != null)
					{
						if (!result.IsSuccess)
						{
							bool displayerror = true;
							if (result.ErrorReason.Contains("few parameters"))
							{
								var cmdembed = Modules.Help.GetCommandHelp(context, arg.Content.Split(' ')[0].Replace(defaultPrefix, "").Replace(customPrefix, ""));
								await MessageHandler.SendChannelAsync(context.Channel, "You seem to be missing a parameter or 2, here's the help", cmdembed);
								displayerror = false;
							}

							if (result.Error != CommandError.UnknownCommand && !result.ErrorReason.Contains("Timeout") && displayerror)
							{
								await Logger.AddToLogsAsync(new Models.LogMessage("CmdHand", "Error with command, Error is: " + result, LogSeverity.Error));
								await MessageHandler.SendChannelAsync(context.Channel, "", new EmbedBuilder { Author = new EmbedAuthorBuilder { Name = "Error with the command" }, Description = Convert.ToString(result.ErrorReason), Color = new Color(255, 0, 0) }.Build());
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
						DogStatsd.Increment("commands.processed");
					}
				}
            }
            catch(Exception ex)
            {
                await Logger.AddToLogsAsync(new Models.LogMessage("CmdHand", "Error with command dispatching", LogSeverity.Error, ex));
            }
        }

		private static async Task<IResult> ExecCommandAsync(ICommandContext context, int argPos, IServiceProvider services = null)
		{
			return await commands.ExecuteAsync(context, argPos, services);
		}

		static bool HasPrefix(SocketUserMessage message, int argPos)
		{
			return message.HasStringPrefix(defaultPrefix, ref argPos) | message.HasStringPrefix(customPrefix, ref argPos);
		}

        private static async Task InsertCommand(CommandInfo command, SocketMessage arg)
        {
            var user = arg.Author;
            var cmd = new MySqlCommand("SELECT UserUsage FROM commandusage WHERE UserID = @userid AND Command = @command");
            cmd.Parameters.AddWithValue("@userid", user.Id);
            cmd.Parameters.AddWithValue("@command", command.Name?? command.Module.Name);
            var resp = await Database.GetSingleAsync(cmd);
            if (!String.IsNullOrEmpty(resp))
            {
                var cmdusg = Convert.ToInt32(resp);
                cmdusg = cmdusg + 1;
                cmd = new MySqlCommand("UPDATE commandusage SET UserUsage = @userusg WHERE UserID = @userid AND Command = @command");
                cmd.Parameters.AddWithValue("@userusg", cmdusg);
                cmd.Parameters.AddWithValue("@userid", user.Id);
                cmd.Parameters.AddWithValue("@command", command.Name ?? command.Module.Name);
                await Database.NonQueryAsync(cmd);
            }
            else
            {
                cmd = new MySqlCommand("INSERT INTO commandusage (`UserID`, `UserUsage`, `Command`) VALUES (@userid , @userusg , @command)");
                cmd.Parameters.AddWithValue("@userusg", 1);
                cmd.Parameters.AddWithValue("@userid", user.Id);
                cmd.Parameters.AddWithValue("@command", command.Name ?? command.Module.Name);
                await Database.NonQueryAsync(cmd);
            }
		}
		private static async Task InsertCommand(CustomCommand command, SocketMessage arg)
		{
			var user = arg.Author;
			var cmd = new MySqlCommand("SELECT UserUsage FROM commandusage WHERE UserID = @userid AND Command = @command");
			cmd.Parameters.AddWithValue("@userid", user.Id);
			cmd.Parameters.AddWithValue("@command", command.CommandName);
			var resp = await Database.GetSingleAsync(cmd);
			if (!String.IsNullOrEmpty(resp))
			{
				var cmdusg = Convert.ToInt32(resp);
				cmdusg = cmdusg + 1;
				cmd = new MySqlCommand("UPDATE commandusage SET UserUsage = @userusg WHERE UserID = @userid AND Command = @command");
				cmd.Parameters.AddWithValue("@userusg", cmdusg);
				cmd.Parameters.AddWithValue("@userid", user.Id);
				cmd.Parameters.AddWithValue("@command", command.CommandName);
				await Database.NonQueryAsync(cmd);
			}
			else
			{
				cmd = new MySqlCommand("INSERT INTO commandusage (`UserID`, `UserUsage`, `Command`) VALUES (@userid , @userusg , @command)");
				cmd.Parameters.AddWithValue("@userusg", 1);
				cmd.Parameters.AddWithValue("@userid", user.Id);
				cmd.Parameters.AddWithValue("@command", command.CommandName);
				await Database.NonQueryAsync(cmd);
			}
		}

		private static bool CheckModule(SkuldGuild guild, ModuleInfo module)
        {
            switch (module.Name)
            {
                case "Accounts":
                    return guild.GuildSettings.Modules.AccountsEnabled;
                case "Actions":
                    return guild.GuildSettings.Modules.ActionsEnabled;
                case "Admin":
                    return guild.GuildSettings.Modules.AdminEnabled;
                case "Fun":
                    return guild.GuildSettings.Modules.FunEnabled;
                case "Help":
                    return guild.GuildSettings.Modules.HelpEnabled;
                case "Information":
                    return guild.GuildSettings.Modules.InformationEnabled;
                case "Search":
                    return guild.GuildSettings.Modules.SearchEnabled;
                case "Stats":
                    return guild.GuildSettings.Modules.StatsEnabled;
                default:
                    return true;
            }
        }
    }
}
