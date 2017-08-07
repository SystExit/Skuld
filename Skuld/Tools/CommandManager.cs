using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Skuld.Tools
{
    class CommandManager : Bot
    {
        public static async Task Bot_MessageReceived(SocketMessage arg)
        {
            if (!arg.Author.IsBot)
            {
                var message = arg as SocketUserMessage;
                string defaultPrefix = Config.Load().Prefix;
                if (message == null) return;
                int argPos = 0;
                var chan = message.Channel as SocketGuildChannel;
                var context = new ShardedCommandContext(bot, message);
                var modules = commands.Modules;
                MySqlCommand command = new MySqlCommand();
                command.CommandText = "SELECT prefix FROM guild WHERE ID = @guildid";
                command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                string customPrefix = await Sql.GetSingleAsync(command);
                if (!String.IsNullOrEmpty(customPrefix))
                {
                    if (message.Content.ToLower() == "skuld.resetprefix")
                    {
                        var guilduser = message.Author as SocketGuildUser;
                        Logs.Add(new Models.LogMessage("HandCmds", $"Prefix reset on a guild", Discord.LogSeverity.Info));
                        if (guilduser.GuildPermissions.ManageGuild)
                        {
                            command = new MySqlCommand();
                            command.CommandText = "UPDATE guild SET prefix = @prefix WHERE ID = @guildid";
                            command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                            command.Parameters.AddWithValue("@prefix", defaultPrefix);
                            await Sql.InsertAsync(command).ContinueWith(async x =>
                            {
                                command = new MySqlCommand();
                                command.CommandText = "SELECT prefix FROM guild WHERE ID = @guildid";
                                command.Parameters.AddWithValue("@guildid", chan.Guild.Id);
                                string newprefix = await Sql.GetSingleAsync(command);
                                if (newprefix == defaultPrefix)
                                    await MessageHandler.SendChannel(message.Channel as SocketTextChannel, $"Successfully reset the prefix, it is now `{newprefix}`");
                            });
                        }
                        else
                            await MessageHandler.SendChannel(message.Channel, "I'm sorry, you don't have permissions to reset the prefix, you need `MANAGE_SERVER`");
                    }
                    else
                        await DispatchCommand(customPrefix, message, context, argPos, arg);
                }
                else
                    await DispatchCommand(defaultPrefix, message, context, argPos, arg);
            }
        }
        private static async Task DispatchCommand(string prefix, SocketUserMessage message, ShardedCommandContext context, int argPos, SocketMessage arg)
        {
            try
            {
                if (!(message.HasStringPrefix(prefix, ref argPos))) return;
                {
                    await InsertCommand(prefix, arg);
                    string[] messagearr = arg.Content.Split(' ');
                    string msg = messagearr[0].Remove(0, prefix.Length);
                    var result = await commands.ExecuteAsync(context, argPos, map);
                    MySqlCommand command = new MySqlCommand();
                    command.CommandText = "UPDATE accounts SET PrevCmd = @command WHERE ID = @userid";
                    command.Parameters.AddWithValue("@command", msg);
                    command.Parameters.AddWithValue("@userid", message.Author.Id);
                    await Sql.InsertAsync(command);
                    if (!result.IsSuccess)
                    {
                        if(result.Error != CommandError.UnknownCommand)
                        {
                            var cmd = commands.Commands.FirstOrDefault(x => x.Name == msg);
                            Logs.Add(new Models.LogMessage("CmdHand", "Error with command, Error is: " + result, Discord.LogSeverity.Error));
                            await MessageHandler.SendChannel(context.Channel, "", new Discord.EmbedBuilder() { Author = new Discord.EmbedAuthorBuilder() { Name = "Error with the command" }, Description = Convert.ToString(result.ErrorReason), Color = new Discord.Color(255, 0, 0) });
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Logs.Add(new Models.LogMessage("CmdHand", "Error with command dispatching", Discord.LogSeverity.Error, ex));
            }

        }
        private static async Task InsertCommand(string prefix, SocketMessage arg)
        {
            int prefixlength = prefix.Length;
            var user = arg.Author;
            string[] messagearr = arg.Content.Split(' ');
            string message = messagearr[0].Remove(0,prefixlength);
            message = message.ToLower();
            foreach (var item in commands.Commands)
            {
                if (item.Name != message) { }
                else
                {
                    MySqlCommand command = new MySqlCommand();
                    command.CommandText = "SELECT * FROM commandusage WHERE UserID = @userid AND Command = @command";
                    command.Parameters.AddWithValue("@userid", user.Id);
                    command.Parameters.AddWithValue("@command", message);
                    var respsql = await Sql.GetAsync(command);
                    if (respsql.HasRows)
                    {
                        while(await respsql.ReadAsync())
                        {
                            var cmd = Convert.ToString(respsql["command"]);
                            var cmdusg = Convert.ToUInt64(respsql["UserUsage"]);
                            cmdusg=cmdusg+1;
                            command = new MySqlCommand();
                            command.CommandText = "UPDATE commandusage SET UserUsage = @userusg WHERE UserID = @userid AND Command = @command";
                            command.Parameters.AddWithValue("@userusg", cmdusg);
                            command.Parameters.AddWithValue("@userid", user.Id);
                            command.Parameters.AddWithValue("@command", message);
                            await Sql.InsertAsync(command);
                        }
                        respsql.Close();
                        await Sql.getconn.CloseAsync();
                        return;
                    }
                    else
                    {
                        command = new MySqlCommand();
                        command.CommandText = "INSERT INTO commandusage (`UserID`, `UserUsage`, `Command`) VALUES (@userid , @userusg , @command)";
                        command.Parameters.AddWithValue("@userusg", 1);
                        command.Parameters.AddWithValue("@userid", user.Id);
                        command.Parameters.AddWithValue("@command", message);
                        await Sql.InsertAsync(command);
                        return;
                    }
                }
            }                        
        }
    }
}
