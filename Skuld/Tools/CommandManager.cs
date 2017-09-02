using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Linq;
using MySql.Data.MySqlClient;
using System.IO;
using System.Collections.Generic;

namespace Skuld.Tools
{
    public class CommandManager : Bot
    {
        private static string defaultPrefix = Config.Load().Prefix;
        private static string customPrefix = null;
        public static async Task Bot_MessageReceived(SocketMessage arg)
        {
            if (!arg.Author.IsBot)
            {
                var message = arg as SocketUserMessage;
                if (message == null) return;
                int argPos = 0;
                var chan = message.Channel as SocketGuildChannel;
                var context = new ShardedCommandContext(bot, message);
                if (message.HasMentionPrefix(bot.CurrentUser, ref argPos))
                { try
                    {
                        await HandleAI(context, context.Message.Content);
                    }
                    catch (Exception ex)
                    {
                        Logs.Add(new Models.LogMessage("CmdHand", "Error Handling AI Stuff", LogSeverity.Error, ex));
                    }
                }
                else
                {
                    if(!String.IsNullOrEmpty(Config.Load().SqlDBHost))
                    {
                        MySqlCommand command = new MySqlCommand("SELECT prefix FROM guild WHERE ID = @guildid");
                        command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                        customPrefix = await Sql.GetSingleAsync(command);
                        if (message.Content.ToLower() == $"{bot.CurrentUser.Username.ToLower()}.resetprefix")
                        {
                            var guilduser = message.Author as SocketGuildUser;
                            Logs.Add(new Models.LogMessage("HandCmd", $"Prefix reset on guild {context.Guild.Name}", Discord.LogSeverity.Info));
                            if (guilduser.GuildPermissions.ManageGuild)
                            {
                                command = new MySqlCommand("UPDATE guild SET prefix = @prefix WHERE ID = @guildid");
                                command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                                command.Parameters.AddWithValue("@prefix", defaultPrefix);
                                await Sql.InsertAsync(command).ContinueWith(async x =>
                                {
                                    command = new MySqlCommand("SELECT prefix FROM guild WHERE ID = @guildid");
                                    command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                                    string newprefix = await Sql.GetSingleAsync(command);
                                    if (newprefix == defaultPrefix)
                                        await MessageHandler.SendChannel(message.Channel as SocketTextChannel, $"Successfully reset the prefix, it is now `{newprefix}`");
                                });
                            }
                            else
                                await MessageHandler.SendChannel(message.Channel, "I'm sorry, you don't have permissions to reset the prefix, you need `MANAGE_SERVER` or `ADMINISTRATOR`");
                        }
                    }
                    if (String.IsNullOrEmpty(customPrefix) || String.IsNullOrEmpty(Config.Load().SqlDBHost))
                        customPrefix = defaultPrefix;
                    await DispatchCommand(message, context, argPos, arg);
                }
            }
        }

        public static async Task HandleAI(ICommandContext Context, string chatmessage)
        {
            try
            {
                if (chatmessage.Contains($"{Context.Client.CurrentUser.Id}"))
                {
                    var messagearr = chatmessage.Split(' ');
                    chatmessage = String.Join(" ", messagearr.Skip(1).ToArray());
                }
                bool triggeredphrase = false;
                KeyValuePair<string,string> trigger = new KeyValuePair<string, string>("","");
                foreach(var phrase in Config.Load().TriggerPhrases)
                {
                    if(chatmessage.ToLowerInvariant().Contains(phrase.Key))
                    {
                        triggeredphrase = true;
                        trigger = phrase;
                        break;
                    }
                }
                if(!triggeredphrase)
                {
                    if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
                        await InsertAI(Context.User);
                    if (!File.Exists(PathToUserData + "\\" + Context.User.Id + ".dat"))
                        ChatUser.Predicates.DictionaryAsXML.Save(PathToUserData + "\\" + Context.User.Id + ".dat");
                    ChatUser = new AIMLbot.User(Convert.ToString(Context.User.Id), ChatService);
                    ChatUser.Predicates.loadSettings(PathToUserData + "\\" + Context.User.Id + ".dat");
                    var r = new AIMLbot.Request(chatmessage, ChatUser, ChatService);
                    var userresp = ChatService.Chat(r);
                    var responce = userresp.Output;
                    ChatService.writeToLog(ChatService.LastLogMessage);
                    ChatUser.Predicates.DictionaryAsXML.Save(PathToUserData + "\\" + Context.User.Id + ".dat");
                    await MessageHandler.SendChannel(Context.Channel, $"{Context.User.Mention}, {responce}");
                }
                else
                {
                    await MessageHandler.SendChannel(Context.Channel, $"{Context.User.Mention}, {trigger.Value}");
                }
            }
            catch(Exception ex)
            {
                Logs.Add(new Models.LogMessage("ChatSrvc", "Error has occured.", LogSeverity.Error, ex));
                await MessageHandler.SendChannel(Context.Channel, $"{Context.User.Mention}, I'm sorry but an error has occured. Try again.");
            }
        }

        private static async Task DispatchCommand(SocketUserMessage message, ShardedCommandContext context, int argPos, SocketMessage arg)
        {
            try
            {
                if (message.HasStringPrefix(customPrefix, ref argPos) || message.HasStringPrefix(defaultPrefix, ref argPos))
                {
                    if (!String.IsNullOrEmpty(Config.Load().SqlDBHost))
                        await InsertCommand(customPrefix ?? defaultPrefix, arg);
                    var result = await commands.ExecuteAsync(context, argPos, map);
                    if (!result.IsSuccess)
                    {
                        if (result.Error != CommandError.UnknownCommand)
                        {
                            Logs.Add(new Models.LogMessage("CmdHand", "Error with command, Error is: " + result, LogSeverity.Error));
                            if (result.Error == CommandError.UnmetPrecondition || result.Error == CommandError.BadArgCount)
                                await MessageHandler.SendChannel(context.Channel, "", new EmbedBuilder() { Author = new EmbedAuthorBuilder() { Name = "Error with the command" }, Description = Convert.ToString(result.ErrorReason), Color = new Color(255, 0, 0) });
                        }
                    }
                }
                else { }
            }
            catch(Exception ex)
            {
                Logs.Add(new Models.LogMessage("CmdHand", "Error with command dispatching", LogSeverity.Error, ex));
            }
        }
        private static async Task InsertCommand(string prefix, SocketMessage arg)
        {
            try
            {
                int prefixlength = prefix.Length;
                var user = arg.Author;
                string[] messagearr = arg.Content.Split(' ');
                string message = messagearr[0];
                message = message.ToLower();
                if (message.Contains(prefix))
                    message = message.Replace(prefix, "");
                foreach (var module in commands.Modules)
                {
                    if (message == module.Name)
                    {
                        if (!String.IsNullOrEmpty(messagearr[1]))
                            message = messagearr[1].ToLower();
                        else
                            message = messagearr[0].ToLower();
                        return;
                    }
                    else { }
                }
                foreach (var item in commands.Commands)
                {
                    if (item.Name != message) { }
                    else
                    {
                        MySqlCommand command = new MySqlCommand("SELECT UserUsage FROM commandusage WHERE UserID = @userid AND Command = @command");
                        command.Parameters.AddWithValue("@userid", user.Id);
                        command.Parameters.AddWithValue("@command", message);
                        var resp = await Sql.GetSingleAsync(command);
                        if (!String.IsNullOrEmpty(resp))
                        {
                            var cmdusg = Convert.ToInt32(resp);
                            cmdusg = cmdusg + 1;
                            command = new MySqlCommand("UPDATE commandusage SET UserUsage = @userusg WHERE UserID = @userid AND Command = @command");
                            command.Parameters.AddWithValue("@userusg", cmdusg);
                            command.Parameters.AddWithValue("@userid", user.Id);
                            command.Parameters.AddWithValue("@command", message);
                            await Sql.InsertAsync(command);
                            return;
                        }
                        else
                        {
                            command = new MySqlCommand("INSERT INTO commandusage (`UserID`, `UserUsage`, `Command`) VALUES (@userid , @userusg , @command)");
                            command.Parameters.AddWithValue("@userusg", 1);
                            command.Parameters.AddWithValue("@userid", user.Id);
                            command.Parameters.AddWithValue("@command", message);
                            await Sql.InsertAsync(command);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }
        private static async Task InsertAI(IUser user)
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM commandusage WHERE UserID = @userid AND Command = @command");
            command.Parameters.AddWithValue("@userid", user.Id);
            command.Parameters.AddWithValue("@command", "chat");
            var respsql = await Sql.GetAsync(command);
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
                    await Sql.InsertAsync(command);
                }
                respsql.Close();
                await Sql.getconn.CloseAsync();
            }
            else
            {
                command = new MySqlCommand("INSERT INTO commandusage (`UserID`, `UserUsage`, `Command`) VALUES (@userid , @userusg , @command)");
                command.Parameters.AddWithValue("@userusg", 1);
                command.Parameters.AddWithValue("@userid", user.Id);
                command.Parameters.AddWithValue("@command", "chat");
                await Sql.InsertAsync(command);
            }
        }
    }
}
