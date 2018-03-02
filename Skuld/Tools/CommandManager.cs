using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Linq;
using MySql.Data.MySqlClient;
using StatsdClient;
using Skuld.Models;

namespace Skuld.Tools
{
    public class CommandManager : Bot
    {
        private static string defaultPrefix = Configuration.Prefix;
        private static string customPrefix;
        public static async Task Bot_MessageReceived(SocketMessage arg)
        {
            if (!arg.Author.IsBot)
            {
                DogStatsd.Increment("messages.recieved");
                var message = arg as SocketUserMessage;

                if (message == null) return;

                int argPos = 0;
                var chan = message.Channel as SocketGuildChannel;
                var context = new ShardedCommandContext(bot, message);

                if (!String.IsNullOrEmpty(Configuration.SqlDBHost))
                {
                    await CheckUser(context.User);

                    var command = new MySqlCommand("SELECT prefix FROM guild WHERE ID = @guildid");
                    command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                    customPrefix = await Database.GetSingleAsync(command);

                    if (message.Content.ToLower() == $"{bot.CurrentUser.Username.ToLower()}.resetprefix")
                    {
                        var guilduser = message.Author as SocketGuildUser;
                        await Logger.AddToLogs(new Models.LogMessage("HandCmd", $"Prefix reset on guild {context.Guild.Name}", LogSeverity.Info));
                        if (guilduser.GuildPermissions.ManageGuild)
                        {
                            command = new MySqlCommand("UPDATE guild SET prefix = @prefix WHERE ID = @guildid");
                            command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                            command.Parameters.AddWithValue("@prefix", defaultPrefix);
                            await Database.NonQueryAsync(command).ContinueWith(async x =>
                            {
                                command = new MySqlCommand("SELECT prefix FROM guild WHERE ID = @guildid");
                                command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                                string newprefix = await Database.GetSingleAsync(command);
                                if (newprefix == defaultPrefix)
                                {
                                    await MessageHandler.SendChannelAsync(message.Channel as SocketTextChannel, $"Successfully reset the prefix, it is now `{newprefix}`");
                                }
                                DogStatsd.Increment("commands.processed");
                            });
                        }
                        else
                        {
                            DogStatsd.Increment("commands.errors.unm-precon");
                            await MessageHandler.SendChannelAsync(message.Channel, "I'm sorry, you don't have permissions to reset the prefix, you need `MANAGE_SERVER` or `ADMINISTRATOR`");
                        }
                    }
                }

                if (String.IsNullOrEmpty(customPrefix) || String.IsNullOrEmpty(Configuration.SqlDBHost))
                {
                    customPrefix = defaultPrefix;
                }

                await DispatchCommand(message, context, argPos, arg);
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
                        await Logger.AddToLogs(new Models.LogMessage("ChkUsr", $"Added {usr.Username} to database", LogSeverity.Info));
                    }
                    else
                    {
                        await Logger.AddToLogs(new Models.LogMessage("ChkUsr", $"Couldn't fix user from being missing; {res.Error}", LogSeverity.Error));
                    }
                }
            }
            catch(Exception ex)
            {
                await Logger.AddToLogs(new Models.LogMessage("Cmd-Mgr","Couldn't fix user from being missing", LogSeverity.Error, ex));
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
                await Logger.AddToLogs(new Models.LogMessage("Cmd-Mgr", "Couldn't fix guild not existing.", LogSeverity.Error, ex));
            }
        }

        private static async Task DispatchCommand(SocketUserMessage message, ShardedCommandContext context, int argPos, SocketMessage arg)
        {
            try
            {
                if (message.HasStringPrefix(customPrefix, ref argPos) || message.HasStringPrefix(defaultPrefix, ref argPos))
                {
                    IResult result = null;
                    CommandInfo cmd = null;
                    if (!String.IsNullOrEmpty(Configuration.SqlDBHost))
                    {
                        var guild = await Database.GetGuildAsync(context.Guild.Id);
                        if(guild!=null)
                        {
							string cmdname = context.Message.Content;
							if (cmdname.StartsWith(customPrefix))
								cmdname = cmdname.Remove(0,customPrefix.Length);
							if (cmdname.StartsWith(defaultPrefix))
								cmdname = cmdname.Remove(0,defaultPrefix.Length);
							var content = cmdname.Split(' ');

							var custcmd = await Database.GetCustomCommandAsync(context.Guild.Id, content[0]);

							if (custcmd != null)
							{
								await MessageHandler.SendChannelAsync(context.Channel, custcmd.Content);
								await InsertCommand(custcmd, message);
							}
							else
							{
								var command = commands.Search(context, arg.Content.Split(' ')[0].Replace(defaultPrefix, "").Replace(customPrefix, "")).Commands;
								if (command == null)
								{
									return;
								}
								else
								{
									var res = command.FirstOrDefault();
									var module = CheckModule(guild, res.Command.Module);
									if (module)
									{
										result = await commands.ExecuteAsync(context, argPos, services);
										cmd = command.FirstOrDefault().Command;
										await InsertCommand(cmd, message);
									}
								}
							}
                        }
                    }
                    else
                    {
                        result = await commands.ExecuteAsync(context, argPos, services);
                    }
                    if(result!=null)
                    {
                        if (!result.IsSuccess)
                        {
                            if (result.Error != CommandError.UnknownCommand)
                            {
                                await Logger.AddToLogs(new Models.LogMessage("CmdHand", "Error with command, Error is: " + result, LogSeverity.Error));
                                await MessageHandler.SendChannelAsync(context.Channel, "", new EmbedBuilder { Author = new EmbedAuthorBuilder { Name = "Error with the command" }, Description = Convert.ToString(result.ErrorReason), Color = new Color(255, 0, 0) }.Build());
                            }
                            DogStatsd.Increment("commands.errors");
                            if (result.Error == CommandError.MultipleMatches)
                            { DogStatsd.Increment("commands.errors.mul-matches"); }
                            if (result.Error == CommandError.UnmetPrecondition)
                            { DogStatsd.Increment("commands.errors.unm-precon"); }
                            if (result.Error == CommandError.Unsuccessful)
                            { DogStatsd.Increment("commands.errors.generic"); }
                            if (result.Error == CommandError.MultipleMatches)
                            { DogStatsd.Increment("commands.errors.multiple"); }
                            if (result.Error == CommandError.BadArgCount)
                            { DogStatsd.Increment("commands.errors.incorr-args"); }
                            if (result.Error == CommandError.ParseFailed)
                            { DogStatsd.Increment("commands.errors.parse-fail"); }
                            if (result.Error == CommandError.Exception)
                            { DogStatsd.Increment("commands.errors.exception"); }
                            if (result.Error == CommandError.UnknownCommand)
                            { DogStatsd.Increment("commands.errors.unk-cmd"); }
                        }
                        else
                        {
                            DogStatsd.Increment("commands.processed");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                await Logger.AddToLogs(new Models.LogMessage("CmdHand", "Error with command dispatching", LogSeverity.Error, ex));
            }
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
