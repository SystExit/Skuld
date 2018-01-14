using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Linq;
using MySql.Data.MySqlClient;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using StatsdClient;

namespace Skuld.Tools
{
    public class CommandManager : Bot
    {
        private static string defaultPrefix = Bot.Configuration.Prefix;
        private static string customPrefix;
        public static System.Diagnostics.Stopwatch CommandStopWatch;
        public static async Task Bot_MessageReceived(SocketMessage arg)
        {
            if (!arg.Author.IsBot)
            {
                DogStatsd.Increment("messages.recieved");
                CommandStopWatch = new System.Diagnostics.Stopwatch();
                CommandStopWatch.Start();
                var message = arg as SocketUserMessage;
                if (message == null) return;
                int argPos = 0;
                var chan = message.Channel as SocketGuildChannel;
                var context = new ShardedCommandContext(bot, message);
                if (!String.IsNullOrEmpty(Bot.Configuration.SqlDBHost))
                {
                    if (String.IsNullOrEmpty(await SqlTools.GetSingleAsync(new MySqlCommand("SELECT `ID` FROM `guild` WHERE `ID` = " + context.Guild.Id))))
                    {
                        await Events.DiscordEvents.PopulateSpecificGuild(context.Guild);
                    }
                    var command = new MySqlCommand("SELECT prefix FROM guild WHERE ID = @guildid");
                    command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                    customPrefix = await SqlTools.GetSingleAsync(command);
                    if (message.Content.ToLower() == $"{bot.CurrentUser.Username.ToLower()}.resetprefix")
                    {
                        var guilduser = message.Author as SocketGuildUser;
                        Logs.Add(new Models.LogMessage("HandCmd", $"Prefix reset on guild {context.Guild.Name}", Discord.LogSeverity.Info));
                        if (guilduser.GuildPermissions.ManageGuild)
                        {
                            command = new MySqlCommand("UPDATE guild SET prefix = @prefix WHERE ID = @guildid");
                            command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                            command.Parameters.AddWithValue("@prefix", defaultPrefix);
                            await SqlTools.InsertAsync(command).ContinueWith(async x =>
                            {
                                command = new MySqlCommand("SELECT prefix FROM guild WHERE ID = @guildid");
                                command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                                string newprefix = await SqlTools.GetSingleAsync(command);
                                if (newprefix == defaultPrefix)
                                    await MessageHandler.SendChannel(message.Channel as SocketTextChannel, $"Successfully reset the prefix, it is now `{newprefix}`");
                                DogStatsd.Increment("commands.processed");
                            });
                        }
                        else
                        {
                            StatsdClient.DogStatsd.Increment("commands.errors.unm-precon");
                            await MessageHandler.SendChannel(message.Channel, "I'm sorry, you don't have permissions to reset the prefix, you need `MANAGE_SERVER` or `ADMINISTRATOR`");
                        }
                    }
                }
                if (String.IsNullOrEmpty(customPrefix) || String.IsNullOrEmpty(Bot.Configuration.SqlDBHost))
                    customPrefix = defaultPrefix;
                await DispatchCommand(message, context, argPos, arg);
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
                    if (!String.IsNullOrEmpty(Bot.Configuration.SqlDBHost))
                    {
                        var guild = await SqlTools.GetGuildAsync(context.Guild.Id);
                        var command = commands.Search(context, arg.Content.Split(' ')[0].Replace(defaultPrefix, "").Replace(customPrefix, "")).Commands;
                        if (command == null) return;
                        {
                            var res = command.FirstOrDefault();
                            var module = CheckModule(guild, res.Command.Module);
                            if (module)
                            {
                                result = await commands.ExecuteAsync(context, argPos, map);
                                cmd = command.FirstOrDefault().Command;
                            }
                        }
                    }
                    else
                    {
                        result = await commands.ExecuteAsync(context, argPos, map);
                    }
                    if(result!=null)
                    {
                        if (!result.IsSuccess)
                        {
                            if (result.Error != CommandError.UnknownCommand)
                            {
                                Logs.Add(new Models.LogMessage("CmdHand", "Error with command, Error is: " + result, LogSeverity.Error));
                                await MessageHandler.SendChannel(context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error with the command" }, Description = Convert.ToString(result.ErrorReason), Color = new Color(255, 0, 0) }.Build());
                            }
                            DogStatsd.Increment("commands.errors");
                            if(result.Error == CommandError.MultipleMatches)
                                DogStatsd.Increment("commands.errors.mul-matches");
                            if (result.Error == CommandError.UnmetPrecondition)
                                DogStatsd.Increment("commands.errors.unm-precon");
                            if (result.Error == CommandError.Unsuccessful)
                                DogStatsd.Increment("commands.errors.generic");
                            if (result.Error == CommandError.MultipleMatches)
                                DogStatsd.Increment("commands.errors.multiple");
                            if (result.Error == CommandError.BadArgCount)
                                DogStatsd.Increment("commands.errors.incorr-args");
                            if (result.Error == CommandError.ParseFailed)
                                DogStatsd.Increment("commands.errors.parse-fail");
                            if (result.Error == CommandError.Exception)
                                DogStatsd.Increment("commands.errors.exception");
                            if (result.Error == CommandError.UnknownCommand)
                                DogStatsd.Increment("commands.errors.unk-cmd");
                        }
                        else
                            DogStatsd.Increment("commands.processed");
                    }
                }
                else { }
            }
            catch(Exception ex)
            {
                Logs.Add(new Models.LogMessage("CmdHand", "Error with command dispatching", LogSeverity.Error, ex));
            }
        }
        private static async Task InsertCommand(CommandInfo command, SocketMessage arg)
        {
            var user = arg.Author;
            var cmd = new MySqlCommand("SELECT UserUsage FROM commandusage WHERE UserID = @userid AND Command = @command");
            cmd.Parameters.AddWithValue("@userid", user.Id);
            cmd.Parameters.AddWithValue("@command", command.Name?? command.Module.Name);
            var resp = await SqlTools.GetSingleAsync(cmd);
            if (!String.IsNullOrEmpty(resp))
            {
                var cmdusg = Convert.ToInt32(resp);
                cmdusg = cmdusg + 1;
                cmd = new MySqlCommand("UPDATE commandusage SET UserUsage = @userusg WHERE UserID = @userid AND Command = @command");
                cmd.Parameters.AddWithValue("@userusg", cmdusg);
                cmd.Parameters.AddWithValue("@userid", user.Id);
                cmd.Parameters.AddWithValue("@command", command.Name ?? command.Module.Name);
                await SqlTools.InsertAsync(cmd);
                return;
            }
            else
            {
                cmd = new MySqlCommand("INSERT INTO commandusage (`UserID`, `UserUsage`, `Command`) VALUES (@userid , @userusg , @command)");
                cmd.Parameters.AddWithValue("@userusg", 1);
                cmd.Parameters.AddWithValue("@userid", user.Id);
                cmd.Parameters.AddWithValue("@command", command.Name ?? command.Module.Name);
                await SqlTools.InsertAsync(cmd);
                return;
            }
        }
        private static async Task InsertAI(IUser user)
        {
            var command = new MySqlCommand("SELECT * FROM commandusage WHERE UserID = @userid AND Command = @command");
            command.Parameters.AddWithValue("@userid", user.Id);
            command.Parameters.AddWithValue("@command", "chat");
            var respsql = await SqlTools.GetAsync(command);
            if (respsql.HasRows)
            {
                while (await respsql.ReadAsync())
                {
                    var cmd = Convert.ToString(respsql["command"]);
                    var cmdusg = Convert.ToUInt64(respsql["UserUsage"]);
                    cmdusg = cmdusg + 1;
                    command = new MySqlCommand("UPDATE commandusage SET UserUsage = @userusg WHERE UserID = @userid AND Command = @command");
                    command.Parameters.AddWithValue("@userusg", cmdusg);
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@command", "chat");
                    await SqlTools.InsertAsync(command);
                }
                respsql.Close();
                await SqlTools.getconn.CloseAsync();
            }
            else
            {
                command = new MySqlCommand("INSERT INTO commandusage (`UserID`, `UserUsage`, `Command`) VALUES (@userid , @userusg , @command)");
                command.Parameters.AddWithValue("@userusg", 1);
                command.Parameters.AddWithValue("@userid", user.Id);
                command.Parameters.AddWithValue("@command", "chat");
                await SqlTools.InsertAsync(command);
            }
        }

        private static bool CheckModule (Models.SkuldGuild guild, ModuleInfo module)
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
